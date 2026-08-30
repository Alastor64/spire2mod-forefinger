using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Forefinger.Cards;
using STS2RitsuLib;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Powers;

// 「指令之意」触发的隐藏状态：在下回合开始、抽牌前把一张战斗内临时的
// 「执行-检索」加入手牌，然后移除自身。
[RegisterPower]
public sealed class ForefingerNextTurnPrescript : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;
    protected override bool IsVisibleInternal => false;

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (Owner is null || player.Creature != Owner)
        {
            return;
        }

        var rng = RitsuLibFramework.GetModPlayerRng(
            player,
            Entry.ModId,
            nameof(ForefingerNextTurnPrescript));

        CardModel? generated = CardFactory
            .GetForCombat(player, [new ForefingerExecuteSkim()], 1, rng)
            .FirstOrDefault();

        if (generated is not null)
        {
            await CardPileCmd.AddGeneratedCardToCombat(
                generated,
                PileType.Hand,
                player,
                CardPilePosition.Random);
        }

        await PowerCmd.Remove(this);
    }
}
