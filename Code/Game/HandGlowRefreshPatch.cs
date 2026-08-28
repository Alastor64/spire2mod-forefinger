using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;

namespace Forefinger.Game;

// 手牌发光若依赖「悬停敌人」（如斩杀指令），原版只在状态变化时刷新发光
// （NHandCardHolder.UpdateCard），悬停变化不会触发刷新，导致悬停到新敌人时
// 发光停留在旧状态。这里在悬停进入/离开生物、以及瞄准结束时强制刷新所有手牌发光。
// 注意：拖出瞄准中的卡牌会被移入 _holdersAwaitingQueue，不在 Hand.Holders 里，
// 所以刷新集合要把这些额外来源一并纳入，否则被拖的那张卡永远刷不到。
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

        var holders = new List<NHandCardHolder>(hand.Holders);
        if (hand._holdersAwaitingQueue is { } awaitingQueue)
        {
            holders.AddRange(awaitingQueue);
        }

        if (hand.FocusedHolder is { } focused)
        {
            holders.Add(focused);
        }

        foreach (NHandCardHolder holder in holders.Distinct())
        {
            holder.UpdateCard();
        }
    }
}
