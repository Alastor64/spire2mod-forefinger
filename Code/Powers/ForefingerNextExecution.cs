using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Forefinger.Keywords;
using STS2RitsuLib;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Powers;

// 下回合执行：在下回合开始、抽牌前，把记录的指令牌复制若干张加入手牌。
// 按设计「不可叠加」：无论记录的卡牌 ID 是否相同，每次应用都新建独立实例、分开显示。
// 只记录卡牌 ID；加入的牌默认为未升级，且只在战斗内存在；加完牌后移除自身。
[RegisterPower]
public sealed class ForefingerNextExecution : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
    public override bool AllowNegative => false;

    protected override IEnumerable<string> RegisteredKeywordIds => [ForefingerKeywords.NextExecutionId];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar("Card", string.Empty),
    ];

    // 记录的卡牌 ID（设计文档：该效果需额外记录一个卡牌 ID）。
    public ModelId? SelectedCardId => GetData().CardId;

    public void SetSelectedCard(ModelId cardId)
    {
        GetData().CardId = cardId;
        // 只读注册表中已初始化模板的标题，避免在打出瞬间访问未初始化实例。
        ((StringVar)DynamicVars["Card"]).StringValue =
            ModelDb.GetByIdOrNull<CardModel>(cardId)?.Title ?? string.Empty;
    }

    // 每次应用都新建一个独立实例（InstanceType.Instanced 本身不会合并），实现「不可叠加、分开显示」。
    public static async Task Apply(
        PlayerChoiceContext choiceContext,
        Creature target,
        ModelId cardId,
        int amount,
        CardModel cardSource)
    {
        if (amount <= 0)
        {
            return;
        }

        var created = await PowerCmd.Apply<ForefingerNextExecution>(
            choiceContext,
            target,
            amount,
            target,
            cardSource,
            silent: false);
        if (created is not null)
        {
            created.SetSelectedCard(cardId);
        }
    }

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (Owner is null || player.Creature != Owner)
        {
            return;
        }

        if (SelectedCardId is not { } cardId || Amount <= 0
            || ModelDb.GetByIdOrNull<CardModel>(cardId) is not { } template)
        {
            await PowerCmd.Remove(this);
            return;
        }

        var rng = RitsuLibFramework.GetModPlayerRng(
            player,
            Entry.ModId,
            nameof(ForefingerNextExecution));

        var generated = CardFactory
            .GetForCombat(player, [template], Amount, rng)
            .ToList();

        if (generated.Count > 0)
        {
            await CardPileCmd.AddGeneratedCardsToCombat(
                generated,
                PileType.Hand,
                player,
                CardPilePosition.Random);
        }

        await PowerCmd.Remove(this);
    }

    protected override object InitInternalData()
    {
        return new Data();
    }

    private Data GetData()
    {
        return GetInternalData<Data>();
    }

    private sealed class Data
    {
        public ModelId? CardId { get; set; }
    }
}
