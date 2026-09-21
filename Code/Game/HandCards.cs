using Godot;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Forefinger.Game;

// 手牌节点刷新的公共实现：发光状态与费用显示都靠它重算。对外只暴露 RequestRefresh，
// 因为整手重刷一次要把每张牌的标题/费用/描述/发光全部重算，而触发它的时机很密：
// 手牌堆内容变化（每移动一张牌一次）、悬停生物、瞄准结束。回合结束一次弃掉整手牌、
// 一次性加入多张牌、瞄准时鼠标扫过多个敌人，都会在同一帧里连着请求好几次，
// 逐次同步重刷就会变成帧内尖刺（一次弃 8 张牌 = 8 次整手重刷，手牌更大时更糟）。
// 原版自己也是合并的：CombatStateTracker.NotifyCombatStateChanged 在已有延迟任务时直接返回，
// CallCombatStateChangedDeferred 先 await 一个进程帧再统一通知。这里保持同样的节奏，
// 用 Godot 的帧末延迟调用把同一帧内的多次请求并成一次。
// 拖出瞄准中的卡牌会被移入 _holdersAwaitingQueue，不在 Hand.Holders 里，所以要一并纳入。
public static class HandCards
{
    private static bool _refreshQueued;

    // 请求重刷手牌显示；同一帧内的后续请求会被合并掉。
    public static void RequestRefresh()
    {
        if (_refreshQueued)
        {
            return;
        }

        _refreshQueued = true;
        Callable.From(FlushRefresh).CallDeferred();
    }

    // 帧末执行的实际刷新。手牌或战斗已经结束时 RefreshVisuals 自己会跳过；
    // 这里再兜住异常，避免一次 UI 刷新出错打断 Godot 的消息队列 flush。
    private static void FlushRefresh()
    {
        _refreshQueued = false;

        try
        {
            RefreshVisuals();
        }
        catch (Exception ex)
        {
            Entry.Logger.Error($"Forefinger: 刷新手牌显示失败：{ex}");
        }
    }

    private static void RefreshVisuals()
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
