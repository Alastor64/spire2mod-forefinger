using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Forefinger.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Cards;

// 信仰之刃：若「手牌独一」，造成 3+{C} 点伤害 2 次。
// {C} 为手牌数（不含运行区，即不含正在打出的这张牌）。
// 升级后 {C} 的系数由 1 变为 2：3+{C}*2。
[RegisterCard(typeof(ForefingerCardPool))]
[RegisterCharacterStarterCard(typeof(ForefingerCharacter), 1)]
public sealed class ForefingerBladeOfFaith : ModCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(3m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((card, _) => card.Owner?.PlayerCombatState?.Hand?.Cards.Count ?? 0),
    ];

    public ForefingerBladeOfFaith()
        : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        if (!CardIdentity.IsHandSingleton(Owner))
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitCount(2)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }
}
