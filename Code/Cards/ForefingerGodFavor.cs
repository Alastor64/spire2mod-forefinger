using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using Forefinger.Characters;
using Forefinger.Game;
using STS2RitsuLib;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Cards;

// 眷顾：0 费稀有技能，对象自身。[消耗]；将随机[指令]加入手中直到塞满，升级后获得[固有]。
//
// 「塞满手中」照原版「潦草急就」的口径：目标张数 = 手牌上限 − 当前手牌数。打出中的这张牌
// 此刻已经离开手牌、进了运行区，所以这里数到的就是剩余手牌；手牌上限走 ForefingerHandLimit，
// 与「心理暗示」共用同一个来源，加成自动生效。手牌已经满（差 0）时什么都不加。
//
// 随机指令直接从指令池取（CardPoolModel.AllCards）：以后往指令池加牌会自动进入随机范围，
// 不必回来改这张牌。逐张抽取与原版「白噪声」「万事通」一样走 CardFactory.GetForCombat，
// 它对候选等概率取（可重复），并自带原版生成过滤（CanBeGeneratedInCombat，排除基础/先古/事件
// 稀有度）。生成的牌未升级、只在本场战斗存在，与本 mod「下回合执行」一致。
// 随机数用本 mod 的玩家 RNG，与「指令加护」「下回合执行」同源，可复现。
//
// 升级只加关键词 CardKeyword.Innate：卡面上那行「固有」由原版 CardModel.GetDescriptionForPile
// 自动插到描述第一行之前，正合设计里「第一行之前插入：[固有]」，不必改描述文本。
[RegisterCard(typeof(ForefingerCardPool))]
public sealed class ForefingerGodFavor : ModCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public ForefingerGodFavor()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is not { } owner || owner.PlayerCombatState?.Hand is not { } hand)
        {
            return;
        }

        int missing = ForefingerHandLimit.LimitFor(owner) - hand.Cards.Count;
        if (missing <= 0)
        {
            return;
        }

        // 从 ModelDb.AllCardPools 里取指令池（而不是 ModelDb.CardPool<T>()）：前者在池子没注册时
        // 只是取不到，后者会直接抛异常，而这里抛异常会打断整次出牌结算。
        if (ModelDb.AllCardPools.OfType<ForefingerPrescriptCardPool>().FirstOrDefault() is not { } pool)
        {
            Entry.Logger.Error("眷顾：找不到指令池，跳过本张牌的结算。");
            return;
        }

        var candidates = pool.AllCards.ToList();
        if (candidates.Count == 0)
        {
            return;
        }

        Rng rng = RitsuLibFramework.GetModPlayerRng(owner, Entry.ModId, nameof(ForefingerGodFavor));
        var generated = CardFactory.GetForCombat(owner, candidates, missing, rng).ToList();
        if (generated.Count == 0)
        {
            return;
        }

        await CardPileCmd.AddGeneratedCardsToCombat(generated, PileType.Hand, owner, CardPilePosition.Random);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
