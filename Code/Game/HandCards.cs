using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Forefinger.Game;

// 手牌节点刷新的公共实现：发光状态与费用显示都靠它重算。
// 拖出瞄准中的卡牌会被移入 _holdersAwaitingQueue，不在 Hand.Holders 里，所以要一并纳入。
public static class HandCards
{
    public static void RefreshVisuals()
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
