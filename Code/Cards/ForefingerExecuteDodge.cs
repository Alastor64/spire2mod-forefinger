using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Forefinger.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Forefinger.Cards;

// 执行-躲闪：0 费普通技能，[指令期限]1，[保留]，[消耗]；使目标获得 3 点格挡，
// 升级后 5 点。属于指令池。
//
// 格挡给的是目标玩家自己的生物，照抄尖塔之意的做法调用 CreatureCmd.GainBlock，
// 因此按目标自己的敏捷结算，而不是打出者。GainsBlock 也必须为 true：
// 格挡类附魔（原版 Nimble 等）用它判断这张牌是否吃得上格挡加成。
[RegisterCard(typeof(ForefingerPrescriptCardPool))]
public sealed class ForefingerExecuteDodge : ForefingerDeadlineCard
{
    protected override int InitialDeadline => 1;

    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        base.CanonicalKeywords.Concat([CardKeyword.Retain, CardKeyword.Exhaust]);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        base.CanonicalVars.Concat([new BlockVar(3m, ValueProp.Move)]);

    public ForefingerExecuteDodge()
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

        await CreatureCmd.GainBlock(targetCreature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}
