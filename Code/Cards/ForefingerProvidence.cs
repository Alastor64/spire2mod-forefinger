using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Forefinger.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Cards;

// 护佑：1 费普通技能，对象自身。每有一名敌人，获得 6/9 点格挡一次。
// 「一次」是逐敌独立结算：有几名敌人就获得几次格挡，而不是一次性拿 6×敌人数。
// 因此每次结算各吃一次敏捷加成，也各触发一次原版「获得格挡时」的效果（Hook.AfterBlockGained）。
// 敌人口径与原版「所有敌人」一致（存活且可被攻击），爪牙/幻影只要可被攻击就算一名。
// 与「寒冰」这种逐敌结算的牌同样处理：连续多次获得格挡，按原版 CreatureCmd.GainBlock
// 的说明传 fast: true，收掉每次之间多余的等待。
[RegisterCard(typeof(ForefingerCardPool))]
public sealed class ForefingerProvidence : ModCardTemplate
{
    private const decimal BaseBlock = 6m;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(BaseBlock, ValueProp.Move)
    ];

    public ForefingerProvidence()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combatState || Owner?.Creature is not { } creature)
        {
            return;
        }

        // 结算前先锁定敌人数量：逐次获得格挡期间战斗名单若有变动，次数也不该跟着变。
        int enemyCount = combatState.HittableEnemies.Count;
        for (int i = 0; i < enemyCount; i++)
        {
            await CreatureCmd.GainBlock(creature, DynamicVars.Block, cardPlay, fast: true);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
