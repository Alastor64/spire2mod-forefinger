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

// 乱剑劈砍：随机对敌人造成 4 点伤害 2 次；若「手牌独一」，获得 2 点能量；
// 若抽牌堆至少有 7 张牌，抽 2 张牌。基础版有「消耗」，升级后移除。
[RegisterCard(typeof(ForefingerCardPool))]
public sealed class ForefingerMultislash : ModCardTemplate
{
    private const int BaseDamage = 4;
    private const int HitCount = 2;
    private const int EnergyGain = 2;
    private const int DrawPileThreshold = 7;
    private const int DrawCount = 2;

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            yield return ModKeywordExtensions.GetModCardKeyword(ForefingerKeywords.HandSingletonId);

            if (!IsUpgraded)
            {
                yield return CardKeyword.Exhaust;
            }
        }
    }

    // 与「信仰之刃」一致：打出手牌时这张牌会进入运行区，所以发金光时把它自己排除掉。
    protected override bool ShouldGlowGoldInternal =>
        CardIdentity.IsHandSingletonExcluding(this);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        new RepeatVar(HitCount),
        new EnergyVar(EnergyGain),
        new CardsVar(DrawCount),
    ];

    public ForefingerMultislash()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.RandomEnemy, true)
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
            .TargetingRandomOpponents(combatState, allowDuplicates: true)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .Execute(choiceContext);

        if (CardIdentity.IsHandSingleton(Owner))
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        }

        if (Owner.PlayerCombatState?.DrawPile?.Cards.Count >= DrawPileThreshold)
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级只移除「消耗」，不改变数值；关键词由 CanonicalKeywords 依据 IsUpgraded 动态生成。
    }
}
