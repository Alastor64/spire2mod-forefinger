using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Forefinger.Characters;
using Forefinger.Game;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Cards;

// 心理暗示：罕见能力牌，1 费，对象自身。
// 打出后获得 2 层 buff「心理暗示」（升级后 3 层），即手牌上限 +2 / +3。
// 手牌已满（手牌数 ≥ 手牌上限）时这张牌的费用降为 0，实时判定。
[RegisterCard(typeof(ForefingerCardPool))]
public sealed class ForefingerSelfSuggestion : ModCardTemplate
{
    private const decimal BaseStacks = 2m;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<Forefinger.Powers.ForefingerSelfSuggestion>(BaseStacks),
    ];

    public ForefingerSelfSuggestion()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature is not { } creature)
        {
            return;
        }

        await PowerCmd.Apply<Forefinger.Powers.ForefingerSelfSuggestion>(
            choiceContext,
            creature,
            DynamicVars["ForefingerSelfSuggestion"].BaseValue,
            creature,
            this,
            silent: false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ForefingerSelfSuggestion"].UpgradeValueBy(1m);
    }

    // 费用的显示与实付都会走到这里：CardEnergyCost.GetWithModifiers 在 Global 部分回调
    // AbstractModel.TryModifyEnergyCostInCombat，所以改这里就是所见即所得（打出时也按 0 费结算）。
    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;

        // 只影响手里这张自己；同一张牌在抽牌堆、弃牌堆或其他牌身上时不生效。
        if (!ReferenceEquals(card, this) || Pile?.Type != PileType.Hand || Owner is not { } owner)
        {
            return false;
        }

        if (!ForefingerHandLimit.IsHandFull(owner))
        {
            return false;
        }

        modifiedCost = 0m;
        return true;
    }
}
