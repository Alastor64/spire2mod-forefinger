using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Forefinger.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Forefinger.Cards;

// 执行-恍惚：0 费技能，[指令期限]1，[保留]，[消耗]；使目标下一回合获得 1 层[脆弱]，
// 升级后 2 层。属于指令池。
[RegisterCard(typeof(ForefingerPrescriptCardPool))]
public sealed class ForefingerExecuteAbsentminded : ForefingerDeadlineCard
{
    protected override int InitialDeadline => 1;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        base.CanonicalKeywords.Concat([CardKeyword.Retain, CardKeyword.Exhaust]);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        base.CanonicalVars.Concat([new PowerVar<Powers.ForefingerAbsentminded>(1m)]);

    public ForefingerExecuteAbsentminded()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyPlayer, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player? targetPlayer = ResolveTargetPlayer(cardPlay.Target);
        if (targetPlayer?.Creature is not { } targetCreature)
        {
            return;
        }

        await Powers.ForefingerAbsentminded.Apply(
            choiceContext,
            targetCreature,
            DynamicVars["ForefingerAbsentminded"].BaseValue,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ForefingerAbsentminded"].UpgradeValueBy(1m);
    }
}
