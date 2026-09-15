using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Powers;

// 「下回合生效」类效果的基类，恍惚、战栗、荒诞共用；TPower 是子类要延迟兑现的那份效果：
//   恍惚 → FrailPower（脆弱）、战栗 → VulnerablePower（易伤）、荒诞 → HelicalDartPower（临时敏捷）。
// 1. 不可为负、可叠加（Counter），层数即「下回合要兑现的份量」。
// 2. 在持有者的下回合开始（抽牌前）把全部层数一次性转成等量份的 TPower，然后移除自身。
// 3. 自己是 buff 还是 debuff 不在这里写死，而是跟着 TPower 走：
//    施加增益（临时敏捷）就是 buff，施加减益（脆弱、易伤）就是 debuff。
// 注意：打出时施加的是这份延迟效果本身，由卡牌直接写 PowerCmd.Apply<具体的延迟效果>()；
// 不能写成 Apply<TPower>，那会当场施加它下回合才该兑现的那份效果。
public abstract class ForefingerNextTurnPower<TPower> : ModPowerTemplate
    where TPower : PowerModel
{
    private PowerType? _type;

    public override PowerType Type => _type ??= ModelDb.Power<TPower>().Type;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => false;

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
            await PowerCmd.Apply<TPower>(choiceContext, Owner, Amount, Owner, null, silent: false);
        }
        finally
        {
            await PowerCmd.Remove(this);
        }
    }
}
