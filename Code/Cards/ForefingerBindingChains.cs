using MegaCrit.Sts2.Core.Combat;
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

// 铁索捆缚：所有敌人永久失去 1 点力量（可把力量扣成负数）；
// 若「手牌独一」，所有敌人本回合再失去 12 点力量（照抄黑暗镣铐的「临时失去力量」）。
// 基础版有「消耗」，升级后移除。
[RegisterCard(typeof(ForefingerCardPool))]
public sealed class ForefingerBindingChains : ModCardTemplate
{
    private const decimal BaseStrengthLoss = 1m;
    private const decimal TurnStrengthLoss = 12m;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordExtensions.GetModCardKeyword(ForefingerKeywords.HandSingletonId),
        CardKeyword.Exhaust,
    ];

    // 与「信仰」「残酷刑罚」一致：独一满足时发光（把自己排除）。
    protected override bool ShouldGlowGoldInternal =>
        CardIdentity.IsHandSingletonExcluding(this);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(BaseStrengthLoss),
        new PowerVar<DarkShacklesPower>(TurnStrengthLoss),
    ];

    public ForefingerBindingChains()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combatState)
        {
            return;
        }

        // 第一行：所有敌人永久失去 1 点力量。
        // 与原版「萎靡」「共赴」一致：把正值取负后施加到 StrengthPower，
        // 因此力量可以被扣成负数。
        foreach (var enemy in combatState.HittableEnemies)
        {
            await PowerCmd.Apply<StrengthPower>(
                choiceContext,
                enemy,
                -DynamicVars["StrengthPower"].BaseValue,
                Owner.Creature,
                this,
                silent: false);
        }

        // 判定时机与「信仰」相同：本牌已进入运行区，判定时不再把自己算入。
        if (!CardIdentity.IsHandSingleton(Owner))
        {
            return;
        }

        // 第二行：照抄「黑暗镣铐」——对每个敌人施加同款
        // TemporaryStrengthPower（至本回合结束时解除，并自动归还力量）。
        foreach (var enemy in combatState.HittableEnemies)
        {
            await PowerCmd.Apply<DarkShacklesPower>(
                choiceContext,
                enemy,
                DynamicVars["DarkShacklesPower"].BaseValue,
                Owner.Creature,
                this,
                silent: false);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
