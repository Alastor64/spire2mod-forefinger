using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Forefinger.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Forefinger.Cards;

// 执行-杀灭：0 费普通攻击，[指令期限]1，[保留]，[消耗]；对所有敌人造成 1 点伤害 3 次，
// 升级后 4 次。属于指令池。多次结算照抄全数歼灭的 WithHitCount。
[RegisterCard(typeof(ForefingerPrescriptCardPool))]
public sealed class ForefingerExecuteExterminate : ForefingerDeadlineCard
{
    protected override int InitialDeadline => 1;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        base.CanonicalKeywords.Concat([CardKeyword.Retain, CardKeyword.Exhaust]);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        base.CanonicalVars.Concat([new DamageVar(1, ValueProp.Move), new RepeatVar(3)]);

    public ForefingerExecuteExterminate()
        : base(0, CardType.Attack, CardRarity.Common, TargetType.AllEnemies, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combatState)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(combatState)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}
