using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Patching.Models;
using Forefinger.Cards;

namespace Forefinger.Game;

// [指令期限] 卡牌的费用不可因任何原因改变（附魔、效果、药水、侵蚀、卡牌）。
// 所有费用修改最终都会落到 CardEnergyCost.GetAmountToSpend，因此在这里强制返回
// 这张牌的规范费用，即可阻断所有减费/加费效果。
public sealed class DeadlineCostPatch : IPatchMethod
{
    public static string PatchId => "forefinger_deadline_cost";
    public static string Description => "Prevent cost changes on cards with the Prescript Deadline keyword.";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        PatchTarget.Method<CardEnergyCost>(nameof(CardEnergyCost.GetAmountToSpend)),
    ];

    public static bool Prefix(CardEnergyCost __instance, ref int __result)
    {
        if (__instance._card is not ForefingerDeadlineCard)
        {
            return true;
        }

        __result = __instance.Canonical;
        return false;
    }
}
