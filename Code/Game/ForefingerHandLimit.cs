using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Forefinger.Game;

// 原版手牌上限是静态常量 CardPile.MaxCardsInHand（当前为 10），引擎没有提供按玩家修改它的钩子，
// 而且全游戏读取它的位置就固定那么几处：引擎的 Draw / Add / SetupPlayerTurn / 抽牌提示，
// 以及原版 6 张「抽到手牌满」类卡牌（Scrawl、Dredge、Pillage、NeowsFury、Anointed、CrashLanding）。
//
// 所以这里做两件事：
//   1) HandLimitPatch 把 CardPile.MaxCardsInHand 的返回值加上心理暗示层数；
//   2) HandLimitScopePatch 在上述读取点登记「此刻算的是谁的手牌」，因为静态属性本身拿不到玩家。
// 这样手牌上限只对心理暗示的持有者生效，多人局里不会互相影响。
public static class ForefingerHandLimit
{
    private static readonly AsyncLocal<Player?> ReadingPlayer = new();

    // 原版常量的实际取值，由 HandLimitPatch 从原方法的返回值里捕获（默认值与当前游戏版本一致）。
    public static int BaseMaxCardsInHand { get; private set; } = 10;

    // 登记「此刻正在计算谁的手牌上限」。
    public static void Begin(Player? player) => ReadingPlayer.Value = player;

    // 由 CardPile.MaxCardsInHand 的后置补丁调用：先记下原版返回值，再按当前玩家加上层数。
    public static int ApplySelfSuggestion(int originalMaxCardsInHand)
    {
        BaseMaxCardsInHand = originalMaxCardsInHand;
        return Limit(originalMaxCardsInHand, ReadingPlayer.Value);
    }

    // 手牌上限：原版常量 + 心理暗示层数，最低 0（上限为 0 时完全无法抽牌）。
    public static int LimitFor(Player? player) => Limit(BaseMaxCardsInHand, player);

    // 「手牌已满」：手牌数 ≥ 手牌上限。判定时这张牌自己也在手里，所以要算进去。
    public static bool IsHandFull(Player? player) =>
        player?.PlayerCombatState?.Hand?.Cards.Count is { } count && count >= LimitFor(player);

    private static int Limit(int baseMaxCardsInHand, Player? player) =>
        Math.Max(0, baseMaxCardsInHand + SelfSuggestionStacks(player));

    private static int SelfSuggestionStacks(Player? player) =>
        player?.Creature?.GetPower<Powers.ForefingerSelfSuggestion>()?.Amount ?? 0;
}
