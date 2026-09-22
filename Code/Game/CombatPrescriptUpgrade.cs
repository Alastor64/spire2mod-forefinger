using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;
using Forefinger.Characters;

namespace Forefinger.Game;

// 「传令员」在本场战斗中升级所有[指令]：
//   · 获得 buff 时整批升级此刻战斗里已有的[指令]（手牌、抽牌堆、弃牌堆、消耗堆）；
//   · 之后进入战斗的[指令]由 buff 的 AfterCardEnteredCombat 单独升级。
//
// 原版战斗中的升级不会随战斗结束自动还原（原版「神化」「武装」都是永久升级），
// 所以这里记下被本 mod 升级的牌，战斗结束时统一降级，与「指令预览」的临时附魔同一套做法。
// 只升级本来就未升级的[指令]：已升级的牌不重复升级；升级也不动[指令期限]计数器，
// 因为计数器是本场战斗里的独立变量，升级只改攻防数值与升级附带的关键词。
internal static class CombatPrescriptUpgrade
{
    private static readonly PileType[] CombatPiles =
        [PileType.Hand, PileType.Draw, PileType.Discard, PileType.Exhaust];

    // 以战斗状态为键，战斗结束后整条丢掉，不会泄漏卡牌引用。
    private static readonly ConditionalWeakTable<ICombatState, List<CardModel>> Upgraded = new();

    // 升级会触发卡牌自身的事件，事件里可能又绕回这里；用标记挡掉重入。
    private static bool _upgrading;

    public static void UpgradeAllFor(Creature? owner)
    {
        if (_upgrading || PlayerOf(owner) is not { } player || owner?.CombatState is not { } combatState)
        {
            return;
        }

        var cards = CardPile.GetCards(player, CombatPiles).Where(IsUpgradeablePrescript).ToList();
        Upgrade(combatState, cards);
    }

    public static void UpgradeCard(CardModel? card)
    {
        if (_upgrading ||
            card?.Owner?.Creature?.CombatState is not { } combatState ||
            !IsUpgradeablePrescript(card))
        {
            return;
        }

        Upgrade(combatState, [card]);
    }

    public static void OnCombatEnded(CombatRoom room)
    {
        if (!Upgraded.TryGetValue(room.CombatState, out var cards))
        {
            return;
        }

        Upgraded.Remove(room.CombatState);

        foreach (CardModel card in cards)
        {
            if (!card.IsUpgraded)
            {
                continue;
            }

            try
            {
                CardCmd.Downgrade(card);
            }
            catch (Exception ex)
            {
                // 单张还原失败不该打断战斗结束的结算。
                Entry.Logger.Error($"传令员：还原[指令]升级失败：{ex}");
            }
        }
    }

    private static bool IsUpgradeablePrescript(CardModel card) =>
        !card.IsUpgraded && card.IsUpgradable && card.Pool is ForefingerPrescriptCardPool;

    private static void Upgrade(ICombatState combatState, IReadOnlyCollection<CardModel> cards)
    {
        if (cards.Count == 0)
        {
            return;
        }

        if (!Upgraded.TryGetValue(combatState, out var tracked))
        {
            tracked = [];
            Upgraded.Add(combatState, tracked);
        }

        foreach (CardModel card in cards)
        {
            if (!tracked.Contains(card))
            {
                tracked.Add(card);
            }
        }

        _upgrading = true;
        try
        {
            // None：整批升级不弹升级预览，只让卡面自己变成升级状态。
            CardCmd.Upgrade(cards, CardPreviewStyle.None);
            HandCards.RequestRefresh();
        }
        finally
        {
            _upgrading = false;
        }
    }

    private static Player? PlayerOf(Creature? creature) =>
        creature?.CombatState?.Players.FirstOrDefault(player => player.Creature == creature);
}
