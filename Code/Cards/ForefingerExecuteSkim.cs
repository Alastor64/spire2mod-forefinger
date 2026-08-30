using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Forefinger.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Forefinger.Cards;

// 执行-检索：0 费技能，[指令期限]0，[保留]，[消耗]；使目标抽 3 张牌，
// 升级后抽 4 张。属于指令池。
[RegisterCard(typeof(ForefingerPrescriptCardPool))]
public sealed class ForefingerExecuteSkim : ForefingerDeadlineCard
{
    protected override int InitialDeadline => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        base.CanonicalKeywords.Concat([CardKeyword.Retain, CardKeyword.Exhaust]);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        base.CanonicalVars.Concat([new CardsVar(3)]);

    public ForefingerExecuteSkim()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.AnyPlayer, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player? targetPlayer = ResolveTargetPlayer(cardPlay.Target);
        if (targetPlayer is null)
        {
            return;
        }

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, targetPlayer);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    private Player? ResolveTargetPlayer(Creature? target)
    {
        if (target is null)
        {
            return Owner;
        }

        if (CombatState is not { } combatState)
        {
            return Owner;
        }

        return combatState.Players.FirstOrDefault(player => player.Creature == target) ?? Owner;
    }
}
