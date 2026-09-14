using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Patching.Models;

namespace Forefinger.Game;

// 手牌上限 = 原版常量 + 当前玩家的心理暗示层数（见 ForefingerHandLimit）。
public sealed class HandLimitPatch : IPatchMethod
{
    public static string PatchId => "forefinger_hand_limit";
    public static string Description => "Add Self Suggestion stacks to the hand size limit.";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        PatchTarget.Getter<CardPile>(nameof(CardPile.MaxCardsInHand)),
    ];

    public static void Postfix(ref int __result) =>
        __result = ForefingerHandLimit.ApplySelfSuggestion(__result);
}

// 登记「此刻在算谁的手牌上限」。目标方法就是全游戏读取 CardPile.MaxCardsInHand 的全部位置，
// 每一条都把该玩家的 Player 交给 ForefingerHandLimit，避免静态属性影响错误的人。
public sealed class HandLimitScopePatch : IPatchMethod
{
    public static string PatchId => "forefinger_hand_limit_scope";
    public static string Description => "Track which player's hand size limit is being read.";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        // 引擎侧：抽牌、加牌到手牌、抽牌提示、回合开始。
        PatchTarget.Method(
            typeof(CardPileCmd),
            nameof(CardPileCmd.Draw),
            typeof(PlayerChoiceContext),
            typeof(decimal),
            typeof(Player),
            typeof(bool)),
        PatchTarget.Method(
            typeof(CardPileCmd),
            nameof(CardPileCmd.Add),
            typeof(IEnumerable<CardModel>),
            typeof(CardPile),
            typeof(CardPilePosition),
            typeof(AbstractModel),
            typeof(bool)),
        PatchTarget.Method(
            typeof(CardPileCmd),
            nameof(CardPileCmd.CheckIfDrawIsPossibleAndShowThoughtBubbleIfNot),
            typeof(Player)),
        PatchTarget.Method<CombatManager>(
            nameof(CombatManager.SetupPlayerTurn),
            typeof(Player),
            typeof(HookPlayerChoiceContext)),

        // 原版「抽到手牌满」类卡牌：都按打出者自己的手牌上限结算。
        CardOnPlay<Anointed>(),
        CardOnPlay<CrashLanding>(),
        CardOnPlay<Dredge>(),
        CardOnPlay<NeowsFury>(),
        CardOnPlay<Pillage>(),
        CardOnPlay<Scrawl>(),
    ];

    public static void Prefix(object[] __args) => ForefingerHandLimit.Begin(FindPlayer(__args));

    // 目标方法签名各不相同：抽牌与回合开始直接给 Player，加牌给的是卡牌，原版卡给的是本次出牌。
    private static ModPatchTarget CardOnPlay<TCard>()
        where TCard : CardModel =>
        PatchTarget.Method<TCard>(
            "OnPlay",
            typeof(PlayerChoiceContext),
            typeof(CardPlay));

    private static Player? FindPlayer(object[] args)
    {
        foreach (var arg in args)
        {
            switch (arg)
            {
                case Player player:
                    return player;
                case CardPlay cardPlay:
                    return cardPlay.Card?.Owner;
                case CardModel card:
                    return card.Owner;
                case IEnumerable<CardModel> cards:
                    return cards.FirstOrDefault()?.Owner;
            }
        }

        return null;
    }
}
