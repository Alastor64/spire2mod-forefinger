using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Forefinger.Cards;

// “独一”判定工具。
// 按 keywords.md 的定义：两张牌“完全相同”需要比较升级状态、附魔、
// 具体内容（含任何临时修改）以及类（代码中）。
internal static class CardIdentity
{
    public static bool IsHandSingleton(Player player)
    {
        var playerCombatState = player.PlayerCombatState;
        if (playerCombatState is null)
        {
            return true;
        }

        var handPile = playerCombatState.Hand;
        if (handPile is null)
        {
            return true;
        }

        var hand = handPile.Cards;
        for (int i = 0; i < hand.Count; i++)
        {
            for (int j = i + 1; j < hand.Count; j++)
            {
                if (AreIdentical(hand[i], hand[j]))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static bool AreIdentical(CardModel a, CardModel b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        // 类（代码中）
        if (a.GetType() != b.GetType())
        {
            return false;
        }

        // 升级状态
        if (a.IsUpgraded != b.IsUpgraded)
        {
            return false;
        }

        // 附魔（类型 + 数量）
        if (!SameEnchantment(a.Enchantment, b.Enchantment))
        {
            return false;
        }

        // 具体内容（含任何临时修改内容）
        return SameDynamicVars(a.DynamicVars, b.DynamicVars);
    }

    private static bool SameEnchantment(EnchantmentModel? a, EnchantmentModel? b)
    {
        if (a is null || b is null)
        {
            return a is null && b is null;
        }

        return a.GetType() == b.GetType() && a.Amount == b.Amount;
    }

    private static bool SameDynamicVars(DynamicVarSet a, DynamicVarSet b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }

        var byName = b.Values.ToDictionary(v => v.Name);
        foreach (var va in a.Values)
        {
            if (!byName.TryGetValue(va.Name, out var vb))
            {
                return false;
            }

            if (va.BaseValue != vb.BaseValue || va.EnchantedValue != vb.EnchantedValue)
            {
                return false;
            }
        }

        return true;
    }
}
