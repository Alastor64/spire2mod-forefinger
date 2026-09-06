using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Forefinger.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Cards;

// 荒诞：1 费普通技能。打出时本牌已进入运行区，因此按「剩余手牌数」（不含自己）判奇偶：
//   偶数 → 获得 8/11 点格挡；
//   奇数 → 获得 3/4 层 buff「荒诞」，于下回合开始时转为等量临时敏捷。
[RegisterCard(typeof(ForefingerCardPool))]
public sealed class ForefingerAbsurdity : ModCardTemplate
{
    private const decimal BaseBlock = 8m;
    private const decimal BaseTempDex = 3m;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(BaseBlock, ValueProp.Move),
        new PowerVar<Forefinger.Powers.ForefingerAbsurdity>(BaseTempDex),
    ];

    public ForefingerAbsurdity()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is not { } owner || owner.Creature is not { } creature)
        {
            return;
        }

        // 判定时机与「信仰」一致：本牌已在运行区，手牌数不再包含自己。
        int remainingHand = owner.PlayerCombatState?.Hand?.Cards.Count ?? 0;
        if (remainingHand % 2 == 0)
        {
            await CreatureCmd.GainBlock(creature, DynamicVars.Block, cardPlay);
            return;
        }

        await Forefinger.Powers.ForefingerAbsurdity.Apply(
            choiceContext,
            creature,
            DynamicVars["ForefingerAbsurdity"].BaseValue,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
        DynamicVars["ForefingerAbsurdity"].UpgradeValueBy(1m);
    }
}
