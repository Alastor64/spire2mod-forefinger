using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Forefinger.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Forefinger.Cards;

// 执行-劈砍：0 费普通攻击，[指令期限]1，[保留]，[消耗]；对单体敌人造成 6 点伤害，
// 升级后 9 点。属于指令池。
[RegisterCard(typeof(ForefingerPrescriptCardPool))]
public sealed class ForefingerExecuteSlash : ForefingerDeadlineCard
{
    protected override int InitialDeadline => 1;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        base.CanonicalKeywords.Concat([CardKeyword.Retain, CardKeyword.Exhaust]);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        base.CanonicalVars.Concat([new DamageVar(6, ValueProp.Move)]);

    public ForefingerExecuteSlash()
        : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
