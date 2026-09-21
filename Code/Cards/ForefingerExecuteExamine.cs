using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Forefinger.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Forefinger.Cards;

// 执行-观察：0 费普通技能，[指令期限]3，[消耗]；使目标抽 1 张牌，升级后获得[保留]。
// 属于指令池。
//
// 升级只加关键词 CardKeyword.Retain：卡面上「保留」那一行由原版
// CardModel.GetDescriptionForPile 自动插到描述第一行之前（同非礼勿视、眷顾），
// 不必也不该写进描述文本。
[RegisterCard(typeof(ForefingerPrescriptCardPool))]
public sealed class ForefingerExecuteExamine : ForefingerDeadlineCard
{
    protected override int InitialDeadline => 3;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        base.CanonicalKeywords.Concat([CardKeyword.Exhaust]);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        base.CanonicalVars.Concat([new CardsVar(1)]);

    public ForefingerExecuteExamine()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.AnyPlayer, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player? targetPlayer = ResolveTargetPlayer(cardPlay.Target);
        if (targetPlayer is null)
        {
            return;
        }

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, targetPlayer);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
