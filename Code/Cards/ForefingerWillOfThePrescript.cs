using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Forefinger.Characters;
using Forefinger.Keywords;
using Forefinger.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Cards;

// 指令之意：3 费攻击，造成 13 伤害并获得 13 格挡；
// 若[手牌独一]，下回合开始、抽牌前向手中加入一张战斗内临时的「执行-检索」。
// 升级后伤害和格挡都从 13 变为 19。
[RegisterCard(typeof(ForefingerCardPool))]
public sealed class ForefingerWillOfThePrescript : ModCardTemplate
{
    private const int BaseAmount = 13;
    private const int UpgradeBonus = 6;

    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordExtensions.GetModCardKeyword(ForefingerKeywords.HandSingletonId),
    ];

    protected override bool ShouldGlowGoldInternal =>
        CardIdentity.IsHandSingletonExcluding(this);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseAmount, ValueProp.Move),
        new BlockVar(BaseAmount, ValueProp.Move),
    ];

    public ForefingerWillOfThePrescript()
        : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        if (Owner?.Creature is not { } creature)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        await CreatureCmd.GainBlock(creature, DynamicVars.Block, cardPlay);

        if (!CardIdentity.IsHandSingleton(Owner))
        {
            return;
        }

        await ForefingerNextExecution.Apply(
            choiceContext,
            creature,
            new ForefingerExecuteSkim(),
            1,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(UpgradeBonus);
        DynamicVars.Block.UpgradeValueBy(UpgradeBonus);
    }
}
