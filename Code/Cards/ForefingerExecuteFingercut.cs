using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Forefinger.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Forefinger.Cards;

// 执行-断指：0 费普通技能，[指令期限]0，[保留]，[消耗]；使目标失去 1 生命 5 次，
// 升级后 8 次。属于指令池。
//
// 按设计这是自残牌（原版有能与自残配合的牌），所以走「失去生命」而不是伤害：
// 照抄本 mod 苦行之刃的自伤写法——ValueProp.Unblockable | Unpowered | Move，
// 即不可被格挡、也不吃力量/虚弱/易伤。5 次分别调用，逐次结算，因此会各触发一次
// 「失去生命时」的效果（这也正是设计里写「5 次」而不是「失去 5 生命」的意义）。
[RegisterCard(typeof(ForefingerPrescriptCardPool))]
public sealed class ForefingerExecuteFingercut : ForefingerDeadlineCard
{
    protected override int InitialDeadline => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        base.CanonicalKeywords.Concat([CardKeyword.Retain, CardKeyword.Exhaust]);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        base.CanonicalVars.Concat([new HpLossVar(1m), new RepeatVar(5)]);

    public ForefingerExecuteFingercut()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.AnyPlayer, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player? targetPlayer = ResolveTargetPlayer(cardPlay.Target);
        if (targetPlayer?.Creature is not { } targetCreature)
        {
            return;
        }

        for (int hit = 0; hit < DynamicVars.Repeat.IntValue; hit++)
        {
            await CreatureCmd.Damage(
                choiceContext,
                targetCreature,
                DynamicVars.HpLoss.BaseValue,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(3m);
    }
}
