using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Forefinger.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Cards;

// 尖塔之意：2 费稀有攻击。对所有敌人造成 5/7 点伤害，所有玩家获得 5/7 点格挡，
// 抽 2 张牌，获得 3 点能量。抽牌与能量只归打出者，与设计文本的分行一致。
// 全员格挡照抄原版多人支援牌（Rally/Intercept）的做法：遍历 ICombatState.Players，
// 对每个玩家自己的生物调用 CreatureCmd.GainBlock，因此各玩家的敏捷分别在各自结算。
// 本牌虽有多人效果，但按设计视为单人牌，故不设置 MultiplayerConstraint
// （与「全数歼灭」一致，保持默认的 None）。
[RegisterCard(typeof(ForefingerCardPool))]
public sealed class ForefingerWillOfTheSpire : ModCardTemplate
{
    private const int BaseDamage = 5;
    private const int BaseBlock = 5;
    private const int UpgradeBonus = 2;
    private const int DrawCount = 2;
    private const int EnergyGain = 3;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        new BlockVar(BaseBlock, ValueProp.Move),
        new CardsVar(DrawCount),
        new EnergyVar(EnergyGain),
    ];

    public ForefingerWillOfTheSpire()
        : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combatState)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(combatState)
            .Execute(choiceContext);

        foreach (var player in combatState.Players)
        {
            if (player.Creature is { } creature)
            {
                await CreatureCmd.GainBlock(creature, DynamicVars.Block, cardPlay);
            }
        }

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(UpgradeBonus);
        DynamicVars.Block.UpgradeValueBy(UpgradeBonus);
    }
}
