using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Forefinger.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Cards;

// 成瘾：罕见能力牌，1 费，对象自身。
// 打出后获得 1 层 buff「成瘾」：此后每打出一张[指令]抽 1 张牌，
// 多张时层数叠加、效果累加。升级后获得[固有]。
[RegisterCard(typeof(ForefingerCardPool))]
public sealed class ForefingerAddict : ModCardTemplate
{
    public ForefingerAddict()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature is not { } creature)
        {
            return;
        }

        await PowerCmd.Apply<Forefinger.Powers.ForefingerAddict>(
            choiceContext, creature, 1m, creature, this, silent: false);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
