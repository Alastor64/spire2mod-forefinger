using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Forefinger.Characters;
using Forefinger.Keywords;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Cards;

// 残酷刑罚：若「手牌独一」，给予目标敌人 4 层[脆弱]、2 层[虚弱]、2 层[易伤]。
// 升级后 4→6、两个 2→3。
[RegisterCard(typeof(ForefingerCardPool))]
public sealed class ForefingerExecute : ModCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordExtensions.GetModCardKeyword(ForefingerKeywords.HandSingletonId),
    ];

    // 手牌独一满足时发光（金色），提示玩家此时打出能触发效果。
    protected override bool ShouldGlowGoldInternal =>
        CardIdentity.IsHandSingletonExcluding(this);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FrailPower>(4m),
        new PowerVar<WeakPower>(2m),
        new PowerVar<VulnerablePower>(2m),
    ];

    public ForefingerExecute()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        if (!CardIdentity.IsHandSingleton(Owner))
        {
            return;
        }

        await PowerCmd.Apply<FrailPower>(
            choiceContext, cardPlay.Target, DynamicVars["FrailPower"].BaseValue, Owner.Creature, this, silent: false);
        await PowerCmd.Apply<WeakPower>(
            choiceContext, cardPlay.Target, DynamicVars["WeakPower"].BaseValue, Owner.Creature, this, silent: false);
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext, cardPlay.Target, DynamicVars["VulnerablePower"].BaseValue, Owner.Creature, this, silent: false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FrailPower"].UpgradeValueBy(2m);
        DynamicVars["WeakPower"].UpgradeValueBy(1m);
        DynamicVars["VulnerablePower"].UpgradeValueBy(1m);
    }
}
