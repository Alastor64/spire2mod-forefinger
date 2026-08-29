using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Forefinger.Characters;
using Forefinger.Keywords;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Cards;

// 苦行之刃：随机对敌人造成 {CD} 点伤害 1 次；若[卡组独一]，获得 1 点能量、
// 额外造成 1 次伤害并失去 1 生命。升级后能量、额外伤害次数、失去生命 1→2。
// 结算顺序按设计描述：先基础伤害，再（若独一）获得能量、额外伤害、失去生命。
[RegisterCard(typeof(ForefingerCardPool))]
public sealed class ForefingerProselyteBlade : ModCardTemplate
{
    private const int BaseEnergyGain = 1;
    private const int BaseExtraHits = 1;
    private const decimal BaseHpLoss = 1m;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordExtensions.GetModCardKeyword(ForefingerKeywords.DeckSingletonId),
    ];

    // 卡组独一满足时发光（金色），提示玩家此时打出能触发额外效果。
    protected override bool ShouldGlowGoldInternal =>
        CardIdentity.IsDeckSingleton(Owner);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(0m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((card, _) => CardIdentity.GetDeckCount(card.Owner)),
        new EnergyVar(BaseEnergyGain),
        new RepeatVar(BaseExtraHits),
        new HpLossVar(BaseHpLoss),
    ];

    public ForefingerProselyteBlade()
        : base(3, CardType.Attack, CardRarity.Rare, TargetType.RandomEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combatState)
        {
            return;
        }

        // 第一行：随机对敌人造成 {CD} 伤害 1 次。
        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this)
            .TargetingRandomOpponents(combatState, allowDuplicates: true)
            .Execute(choiceContext);

        if (!CardIdentity.IsDeckSingleton(Owner))
        {
            return;
        }

        // 若[卡组独一]：先获得能量，再额外造成伤害，最后失去生命。
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this)
            .TargetingRandomOpponents(combatState, allowDuplicates: true)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .Execute(choiceContext);

        if (Owner.Creature is { } creature)
        {
            await CreatureCmd.Damage(
                choiceContext,
                creature,
                DynamicVars.HpLoss.BaseValue,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
        DynamicVars.Repeat.UpgradeValueBy(1m);
        DynamicVars.HpLoss.UpgradeValueBy(1m);
    }
}
