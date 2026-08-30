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
// 同一卡牌 ID 叠加，不同 ID 各自独立显示。加入的牌默认为未升级，且只在战斗内存在。
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

    public CardModel? SelectedCard => GetData().SelectedCard;

    public void SetSelectedCard(CardModel card)
    {
        GetData().SelectedCard = card;
        ((StringVar)DynamicVars["Card"]).StringValue = card.Title;
    }

    public static async Task Apply(
        PlayerChoiceContext choiceContext,
        Creature target,
        CardModel card,
        int amount,
        CardModel cardSource)
    {
        if (amount <= 0)
        {
            return;
        }

        ForefingerNextExecution? existing = target.Powers
            .OfType<ForefingerNextExecution>()
            .FirstOrDefault(power => power.SelectedCard?.GetType() == card.GetType());

        if (existing is not null)
        {
            await PowerCmd.Apply(choiceContext, existing, target, amount, target, cardSource, silent: false);
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
            created.SetSelectedCard(card);
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

        if (SelectedCard is not { } selectedCard || Amount <= 0)
        {
            await PowerCmd.Remove(this);
            return;
        }

        var rng = RitsuLibFramework.GetModPlayerRng(
            player,
            Entry.ModId,
            nameof(ForefingerNextExecution));

        var generated = CardFactory
            .GetForCombat(player, [selectedCard], Amount, rng)
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
        public CardModel? SelectedCard { get; set; }
    }
}
