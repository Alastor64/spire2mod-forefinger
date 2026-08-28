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

// 信仰之刃：若「手牌独一」，造成 {C} 点伤害 2 次。
// {C} 为手牌数（不含运行区，即不含正在打出的这张牌）。
// 升级后基础伤害 +2：2+{C}。
[RegisterCard(typeof(ForefingerCardPool))]
[RegisterCharacterStarterCard(typeof(ForefingerCharacter), 1)]
public sealed class ForefingerBladeOfFaith : ModCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordExtensions.GetModCardKeyword(ForefingerKeywords.HandSingletonId),
    ];

    // 手牌独一满足时发光（金色），提示玩家此时打出能触发额外效果。
    protected override bool ShouldGlowGoldInternal =>
        CardIdentity.IsHandSingletonExcluding(this);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(0m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((card, _) =>
            {
                if (card.Owner is null)
                {
                    return 0m;
                }

                // {C} 是「打出后」的手牌数：这张牌还在手里（预览）时要排除它自己，
                // 因为打出时它会进入运行区。与原版「精确切击」计算「其他手牌」同口径。
                var handPile = PileTypeExtensions.GetPile(PileType.Hand, card.Owner);
                decimal handCount = handPile.Cards.Count;
                if (card.Pile is not null && card.Pile.Type == PileType.Hand)
                {
                    handCount -= 1;
                }

                return Math.Max(handCount, 0);
            }),
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
        DynamicVars.CalculationBase.UpgradeValueBy(2m);
    }
}
