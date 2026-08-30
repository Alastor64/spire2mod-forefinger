using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Forefinger.Keywords;
using Forefinger.Powers;
using Forefinger.Relics;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Cards;

// 带 [指令期限] 的卡牌基类：
// 1. 费用不可被任何效果改变（由 DeadlineCostPatch 在最终结算处兜底）。
// 2. 计数器以 DynamicVar "Deadline" 存储，每场战斗开始时重置为初值。
// 3. 被消耗时获得 1 层「解禁中」。
// 4. 回合结束时，若不在消耗堆，则倒计时或令「业报」+1。
public abstract class ForefingerDeadlineCard : ModCardTemplate
{
    public const string DeadlineVarName = "Deadline";

    protected abstract int InitialDeadline { get; }

    protected CardKeyword DeadlineKeyword =>
        ModKeywordExtensions.GetModCardKeyword(ForefingerKeywords.DeadlineId);

    protected ForefingerDeadlineCard(
        int energy,
        CardType type,
        CardRarity rarity,
        TargetType targetType,
        bool isPlayable)
        : base(energy, type, rarity, targetType, isPlayable)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [DeadlineKeyword];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(DeadlineVarName, InitialDeadline),
    ];

    public override Task BeforeCombatStart()
    {
        SetDeadline(InitialDeadline);
        return Task.CompletedTask;
    }

    public override async Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal)
    {
        if (!ReferenceEquals(card, this) || Owner?.Creature is not { } creature)
        {
            return;
        }

        await PowerCmd.Apply<ForefingerUnlocking>(
            choiceContext,
            creature,
            1m,
            creature,
            null,
            silent: false);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || CombatState is null || Owner?.Creature is not { } creature)
        {
            return;
        }

        if (!participants.Contains(creature))
        {
            return;
        }

        // 已经被消耗的牌不再倒计时。
        if (Pile?.Type == PileType.Exhaust)
        {
            return;
        }

        int deadline = GetDeadline();
        if (deadline > 0)
        {
            SetDeadline(deadline - 1);
            return;
        }

        await ForefingerKarma.AddKarma(choiceContext, Owner, 1);
    }

    protected int GetDeadline()
    {
        return DynamicVars[DeadlineVarName].IntValue;
    }

    protected void SetDeadline(decimal value)
    {
        DynamicVars[DeadlineVarName].BaseValue = value;
    }
}
