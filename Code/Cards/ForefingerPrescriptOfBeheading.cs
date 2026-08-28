using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using Forefinger.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Cards;

// 斩杀指令：造成 16 点伤害；若目标生命值严格低于其最大生命值的一半，伤害翻倍。
// 升级后基础伤害 16→24，翻倍后为 48。翻倍与「本能」附魔同口径：翻倍的是基础攻击伤害。
[RegisterCard(typeof(ForefingerCardPool))]
public sealed class ForefingerPrescriptOfBeheading : ModCardTemplate
{
    private const decimal BaseDamage = 16m;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(BaseDamage),
        new ExtraDamageVar(BaseDamage),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((_, target) => IsBelowHalfHp(target) ? 1m : 0m),
    ];

    // 发光规则：未选中此卡时，看是否存在满足条件的敌人；选中（瞄准中）且悬停着
    // 敌人时，只看悬停的那一个；选中但没有悬停敌人时，仍看是否存在满足条件的敌人。
    protected override bool ShouldGlowGoldInternal
    {
        get
        {
            if (NTargetManager.Instance is { IsInSelection: true } targetManager
                && targetManager.HoveredNode is NCreature { Entity: Creature { IsEnemy: true } creature })
            {
                return IsBelowHalfHp(creature);
            }

            if (CombatState is not { } combatState)
            {
                return false;
            }

            return combatState.HittableEnemies.Any(IsBelowHalfHp);
        }
    }

    public ForefingerPrescriptOfBeheading()
        : base(3, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(8m);
        DynamicVars.ExtraDamage.UpgradeValueBy(8m);
    }

    private static bool IsBelowHalfHp(Creature? target)
    {
        if (target is null || target.MaxHp <= 0)
        {
            return false;
        }

        return target.CurrentHp * 2 < target.MaxHp;
    }
}
