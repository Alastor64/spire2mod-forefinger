using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Forefinger.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Cards;

// 非礼勿视：0 费普通技能，对象自身。丢弃手牌中所有状态牌；升级后获得[保留]。
// 「状态牌」按原版口径取 CardType.Status（即「状态」这一卡牌类型），不含诅咒牌，
// 与原版「弹片炮」写「你所有的状态牌」时的判定一致。
// 「丢弃」直接用原版 CardCmd.Discard：它逐张移入弃牌堆，每张各记一次战斗历史并各触发一次
// 原版「弃牌时」的效果（Hook.AfterCardDiscarded，如绷带、丁夏），奇巧牌还会因此免费打出；
// 口径与原版「丢弃所有手牌」的影子步、钢之风暴相同。先取快照再结算，避免边弃边改手牌集合。
// 「保留」是原版关键词（CardKeyword.Retain），升级时加进这张牌即可：卡面上的「保留」
// 由原版 CardModel.GetDescriptionForPile 自动插到描述第一行之前，不必也不该写进描述文本。
[RegisterCard(typeof(ForefingerCardPool))]
public sealed class ForefingerSeeNoEvil : ModCardTemplate
{
    public ForefingerSeeNoEvil()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.PlayerCombatState?.Hand?.Cards is not { } hand)
        {
            return;
        }

        var statuses = hand.Where(card => card.Type == CardType.Status).ToList();
        if (statuses.Count == 0)
        {
            return;
        }

        await CardCmd.Discard(choiceContext, statuses);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
