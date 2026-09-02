using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Enchantments;

// 坚固 Sturdy：打出时获得与层数等量的格挡，独立于卡牌自身的格挡结算，
// 因此格挡类技能（如 防御）会获得两次格挡，且坚固提供的格挡同样受敏捷加成。
// 可叠加：同一张已附坚固的牌再次被附坚固时，层数相加。
[RegisterEnchantment]
public sealed class ForefingerSturdy : ModEnchantmentTemplate
{
    // 数量用 BlockVar 承载：值跟随层数，并带 Move 属性（能吃敏捷等修正）。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(0m, ValueProp.Move),
    ];

    public override bool ShowAmount => true;

    public override bool HasExtraCardText => true;

    // 设计要求层数加和；原版机制下同类型附魔再次施加时把数量累加。
    public override bool IsStackable => true;

    // 每当附有坚固的牌被打出，额外结算一次格挡。
    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        if (Card is not { Owner: { Creature: { } creature } })
        {
            return;
        }

        await CreatureCmd.GainBlock(creature, DynamicVars.Block, cardPlay);
    }

    // 层数变化后同步 BlockVar，让卡面上的额外文本显示正确的数值。
    public override void RecalculateValues()
    {
        DynamicVars.Block.BaseValue = Amount;
    }
}
