using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Forefinger.Game;

// 追踪「指令预览」在本场战斗临时施加的附魔，战斗结束时统一清除，
// 从而把 Sharp/Nimble 这种永久附魔变成“仅限本场战斗”的临时强化。
// 用 ConditionalWeakTable 以战斗状态为键，战斗结束后不会泄漏卡牌引用。
internal static class CombatEnchantTracker
{
    private static readonly ConditionalWeakTable<ICombatState, List<CardModel>> TempEnchanted = new();

    public static void Track(ICombatState combatState, CardModel card)
    {
        if (!TempEnchanted.TryGetValue(combatState, out var cards))
        {
            cards = new List<CardModel>();
            TempEnchanted.Add(combatState, cards);
        }

        cards.Add(card);
    }

    public static void OnCombatEnded(CombatRoom room)
    {
        if (!TempEnchanted.TryGetValue(room.CombatState, out var cards))
        {
            return;
        }

        foreach (var card in cards)
        {
            if (card.Enchantment is not null)
            {
                CardCmd.ClearEnchantment(card);
            }
        }

        TempEnchanted.Remove(room.CombatState);
    }
}
