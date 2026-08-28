using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Forefinger.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Cards;

// 全数歼灭：对所有敌人造成 4 点伤害 3 次；每「斩杀」一次（本牌击杀一名非爪牙敌人），
// 所有玩家及其随从各获得 1 点力量。升级后伤害 4→6、力量 1→2。
[RegisterCard(typeof(ForefingerCardPool))]
public sealed class ForefingerEliminate : ModCardTemplate
{
    private const int BaseDamage = 4;
    private const int HitCount = 3;
    private const decimal BaseStrength = 1m;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        new RepeatVar(HitCount),
        new PowerVar<StrengthPower>(BaseStrength),
    ];

    // 与原版「狂宴」一致，用 Powers.All(ShouldOwnerDeathTriggerFatal) 判断一名敌人是否算
    // 非爪牙敌人：爪牙会携带 MinionPower，该方法返回 false。
    private static bool IsFatalEligible(Creature creature) =>
        creature.Powers.All(power => power.ShouldOwnerDeathTriggerFatal());

    public ForefingerEliminate()
        : base(2, CardType.Attack, CardRarity.Common, TargetType.AllEnemies, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combatState)
        {
            return;
        }

        // 在伤害结算前先锁定哪些敌人算「非爪牙」，避免敌人死亡后其 Powers 被清空而误判。
        var fatalEligibleEnemies = combatState.HittableEnemies
            .Where(IsFatalEligible)
            .ToHashSet();

        var command = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(combatState)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .Execute(choiceContext);

        int killCount = command.Results
            .SelectMany(hit => hit)
            .Where(result => result.WasTargetKilled && fatalEligibleEnemies.Contains(result.Receiver))
            .Select(result => result.Receiver)
            .Distinct()
            .Count();

        if (killCount == 0)
        {
            return;
        }

        var targets = GetStrengthTargets(combatState).ToList();
        if (targets.Count == 0)
        {
            return;
        }

        decimal strength = DynamicVars["StrengthPower"].BaseValue * killCount;
        await PowerCmd.Apply<StrengthPower>(
            choiceContext, targets, strength, Owner.Creature, this, silent: false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["StrengthPower"].UpgradeValueBy(1m);
    }

    private static IEnumerable<Creature> GetStrengthTargets(ICombatState combatState)
    {
        foreach (var player in combatState.Players)
        {
            if (player.Creature is { } creature)
            {
                yield return creature;
            }
        }

        foreach (var creature in combatState.Creatures)
        {
            if (!creature.IsPlayer && creature.PetOwner is not null)
            {
                yield return creature;
            }
        }
    }
}
