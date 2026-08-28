using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;

namespace Forefinger.Game;

// 手牌发光若依赖「悬停敌人」（如斩杀指令），原版只在状态变化时刷新发光
// （NHandCardHolder.UpdateCard），悬停变化不会触发刷新，导致悬停到新敌人时
// 发光停留在旧状态。这里在悬停进入/离开生物、以及瞄准结束时强制刷新所有手牌发光。
public sealed class HandGlowRefreshPatch : IPatchMethod
{
    public static string PatchId => "forefinger_hand_glow_refresh";
    public static string Description => "Refresh in-hand card glow when hovering creatures or finishing targeting.";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        PatchTarget.Method<NTargetManager>(nameof(NTargetManager.OnCreatureHovered), typeof(NCreature)),
        PatchTarget.Method<NTargetManager>(nameof(NTargetManager.OnCreatureUnhovered), typeof(NCreature)),
        PatchTarget.Method<NTargetManager>(nameof(NTargetManager.FinishTargeting), typeof(bool)),
    ];

    public static void Postfix() => RefreshHandGlow();

    private static void RefreshHandGlow()
    {
        if (NPlayerHand.Instance is not { } hand)
        {
            return;
        }

        foreach (NHandCardHolder holder in hand.Holders)
        {
            holder.UpdateCard();
        }
    }
}
