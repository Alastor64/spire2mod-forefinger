using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Forefinger.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Powers;

// 成瘾：可叠加、不可为负的永久 buff。
// 每当持有者打出一张属于指令池的牌，抽与当前层数等量的牌。
// 多次获得时层数合并（如两张「成瘾」使每张[指令]抽 2 张），
// 与尖塔惯例一致：卡面按单张写，多张效果叠加。
[RegisterPower]
public sealed class ForefingerAddict : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is null || Amount <= 0)
        {
            return;
        }

        // 只对持有者本人打出的指令池卡牌生效。
        if (cardPlay.Card is not { } card ||
            card.Owner is not { } player ||
            player.Creature != Owner ||
            card.Pool is not ForefingerPrescriptCardPool)
        {
            return;
        }

        await CardPileCmd.Draw(choiceContext, (int)Amount, player);
    }
}
