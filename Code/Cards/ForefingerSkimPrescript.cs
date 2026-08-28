using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using Forefinger.Characters;
using Forefinger.Game;
using STS2RitsuLib;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Cards;

// 指令预览：固有，打出后随机给卡组中一张未附魔的攻击牌附「锋利 1」、
// 一张未附魔的技能牌附「灵巧 1」，然后消耗。升级后额外抽一张牌。
// 附魔仅在本场战斗内生效，战斗结束统一清除。
[RegisterCard(typeof(ForefingerCardPool))]
[RegisterCharacterStarterCard(typeof(ForefingerCharacter), 1)]
public sealed class ForefingerSkimPrescript : ModCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Innate,
        CardKeyword.Exhaust,
    ];

    public ForefingerSkimPrescript()
        : base(0, CardType.Skill, CardRarity.Basic, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combatState)
        {
            return;
        }

        var rng = RitsuLibFramework.GetModPlayerRng(Owner, Entry.ModId, nameof(ForefingerSkimPrescript));

        EnchantRandomCard<Sharp>(Owner, rng, CardType.Attack, combatState);
        EnchantRandomCard<Nimble>(Owner, rng, CardType.Skill, combatState);

        if (IsUpgraded)
        {
            await CardPileCmd.Draw(choiceContext, 1, Owner);
        }
    }

    private static void EnchantRandomCard<T>(
        Player player,
        MegaCrit.Sts2.Core.Random.Rng rng,
        CardType type,
        MegaCrit.Sts2.Core.Combat.ICombatState combatState)
        where T : EnchantmentModel
    {
        var playerCombatState = player.PlayerCombatState;
        if (playerCombatState is null)
        {
            return;
        }

        // 卡组 = 抽牌堆 + 弃牌堆 + 手牌（不含运行区/消耗堆，见 design 备注）。
        // 不能用 AllCards，因为它包含 PlayPile（正在打出的牌），会把「正在打出的
        // 指令预览自己」也当作候选，随机选中时对正在打出的牌附魔会卡住。
        var deckCards = new List<CardModel>();
        AddPile(deckCards, playerCombatState.DrawPile);
        AddPile(deckCards, playerCombatState.DiscardPile);
        AddPile(deckCards, playerCombatState.Hand);

        var candidates = deckCards
            .Where(card => card.Type == type && card.Enchantment is null)
            .ToList();

        if (candidates.Count == 0)
        {
            return;
        }

        var target = rng.NextItem(candidates);
        if (target is null)
        {
            return;
        }

        var enchantment = CardCmd.Enchant<T>(target, 1m);
        if (enchantment is not null)
        {
            CombatEnchantTracker.Track(combatState, target);
        }
    }

    private static void AddPile(List<CardModel> cards, CardPile? pile)
    {
        if (pile is not null)
        {
            cards.AddRange(pile.Cards);
        }
    }
}
