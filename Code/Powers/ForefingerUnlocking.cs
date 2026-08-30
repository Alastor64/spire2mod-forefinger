using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Forefinger.Keywords;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Powers;

// 解禁中：可叠加的计数器。达到 7 层时获得「剑刃解放」，跌破 7 层时失去；
// 回合结束时层数减 1，从获得它的当回合结束开始递减。
[RegisterPower]
public sealed class ForefingerUnlocking : ModPowerTemplate
{
    private const int Threshold = 7;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    protected override IEnumerable<string> RegisteredKeywordIds => [ForefingerKeywords.UnlockingId];

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (!ReferenceEquals(power, this) || Owner is not { } owner)
        {
            return;
        }

        decimal previous = Math.Max(power.Amount - amount, 0m);
        decimal current = power.Amount;

        if (previous < Threshold && current >= Threshold)
        {
            await ApplyBladeUnlocked(choiceContext, owner);
        }
        else if (previous >= Threshold && current < Threshold)
        {
            await RemoveBladeUnlocked(choiceContext, owner);
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || Owner is not { } owner || !participants.Contains(owner))
        {
            return;
        }

        if (Amount <= 0)
        {
            return;
        }

        await PowerCmd.ModifyAmount(choiceContext, this, -1m, owner, null, silent: false);
    }

    private async Task ApplyBladeUnlocked(PlayerChoiceContext choiceContext, Creature owner)
    {
        var existingBlade = owner.Powers.OfType<ForefingerBladeUnlocked>().FirstOrDefault();
        if (existingBlade is null)
        {
            await PowerCmd.Apply<ForefingerBladeUnlocked>(
                choiceContext, owner, 1m, owner, null, silent: false);
        }

        await PowerCmd.Apply<StrengthPower>(
            choiceContext, owner, 1m, owner, null, silent: false);
        await PowerCmd.Apply<DexterityPower>(
            choiceContext, owner, 1m, owner, null, silent: false);
    }

    private async Task RemoveBladeUnlocked(PlayerChoiceContext choiceContext, Creature owner)
    {
        var existingBlade = owner.Powers.OfType<ForefingerBladeUnlocked>().FirstOrDefault();
        if (existingBlade is not null)
        {
            await PowerCmd.Remove(existingBlade);
        }

        var strength = owner.Powers.OfType<StrengthPower>().FirstOrDefault();
        if (strength is not null)
        {
            await PowerCmd.ModifyAmount(choiceContext, strength, -1m, owner, null, silent: false);
        }

        var dexterity = owner.Powers.OfType<DexterityPower>().FirstOrDefault();
        if (dexterity is not null)
        {
            await PowerCmd.ModifyAmount(choiceContext, dexterity, -1m, owner, null, silent: false);
        }
    }
}
