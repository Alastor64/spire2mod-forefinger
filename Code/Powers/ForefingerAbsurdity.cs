using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Powers;

// 荒诞：可叠加、不可为负的延迟 buff。
// 在持有者的下回合开始（抽牌前）把全部层数转为等量「本回合结束前获得」的临时敏捷，然后移除自身。
[RegisterPower]
public sealed class ForefingerAbsurdity : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public static async Task Apply(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        CardModel cardSource)
    {
        if (amount <= 0)
        {
            return;
        }

        await PowerCmd.Apply<ForefingerAbsurdity>(
            choiceContext, target, amount, target, cardSource, silent: false);
    }

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (Owner is null || player.Creature != Owner)
        {
            return;
        }

        if (Amount <= 0)
        {
            await PowerCmd.Remove(this);
            return;
        }

        try
        {
            // 照抄原版「螺旋镖」：施加正值的 HelicalDartPower，
            // 即立刻获得与层数等量的敏捷，持续至本回合结束时自动解除。
            await PowerCmd.Apply<HelicalDartPower>(
                choiceContext,
                Owner,
                Amount,
                Owner,
                null,
                silent: false);
        }
        finally
        {
            await PowerCmd.Remove(this);
        }
    }
}
