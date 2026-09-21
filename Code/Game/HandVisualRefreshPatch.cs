using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Patching.Models;

namespace Forefinger.Game;

// 「心理暗示」让手牌上限变化后，「手牌已满则费用为 0」的判定可能在抽牌/弃牌/出牌时翻转，
// 而原版只在卡牌自身状态变化时重算手牌费用。这里在手牌堆内容变化后重刷手牌显示；
// 刷新请求由 HandCards 合并，同一帧内移动多张牌只重刷一次（原版自己也是合并 + 延迟一帧）。
public sealed class HandVisualRefreshPatch : IPatchMethod
{
    public static string PatchId => "forefinger_hand_visual_refresh";
    public static string Description => "Refresh in-hand card visuals when the hand pile changes.";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        PatchTarget.Method<CardPile>(nameof(CardPile.InvokeContentsChanged)),
    ];

    public static void Postfix(CardPile __instance)
    {
        if (__instance.Type == PileType.Hand)
        {
            HandCards.RequestRefresh();
        }
    }
}
