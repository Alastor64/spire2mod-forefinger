using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;
using Forefinger.Characters;
using STS2RitsuLib;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Relics;

// 业报：跨战斗保留的计数器遗物。每层业报由 [指令期限] 倒计时归零产生。
// 计数器达到 20 时随机触发一项惩罚并减 20；没有上限，因此 60 会连续触发三次。
[RegisterRelic(typeof(ForefingerRelicPool))]
[RegisterCharacterStarterRelic(typeof(ForefingerCharacter))]
public sealed class ForefingerKarma : ModRelicTemplate
{
    private const int Threshold = 20;
    private const int PunishmentCount = 3;

    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool ShowCounter => true;
    public override int DisplayAmount => DynamicVars["Counter"].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Counter", 0m),
    ];

    // 不进入普通遗物池，只作为食指的初始事件遗物自动获得。
    public override bool IsAllowed(IRunState runState) => false;

    public static async Task AddKarma(PlayerChoiceContext choiceContext, Player player, int amount)
    {
        if (amount <= 0 || player.GetRelic<ForefingerKarma>() is not { } karma)
        {
            return;
        }

        int counter = karma.GetCounter() + amount;
        while (counter >= Threshold)
        {
            counter -= Threshold;
            karma.SetCounter(counter);
            await karma.TriggerRandomPunishment(choiceContext, player);
        }

        karma.SetCounter(counter);
    }

    private int GetCounter()
    {
        return DynamicVars["Counter"].IntValue;
    }

    private void SetCounter(int value)
    {
        DynamicVars["Counter"].BaseValue = Math.Max(value, 0);
        InvokeDisplayAmountChanged();
    }

    private async Task TriggerRandomPunishment(PlayerChoiceContext choiceContext, Player player)
    {
        var rng = RitsuLibFramework.GetModPlayerRng(player, Entry.ModId, nameof(ForefingerKarma));

        switch (rng.NextInt(PunishmentCount))
        {
            case 0:
                await LoseMaxHp(choiceContext, player);
                break;
            case 1:
                await AddRegret(player);
                break;
            case 2:
                await RemoveWeightedCard(player, rng);
                break;
            default:
                Entry.Logger.Error($"业报收到了无效的惩罚编号。");
                break;
        }
    }

    private static async Task LoseMaxHp(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature is not { } creature)
        {
            return;
        }

        decimal loss = Math.Min(10m, Math.Max(creature.MaxHp - 1m, 0m));
        if (loss <= 0m)
        {
            return;
        }

        await CreatureCmd.LoseMaxHp(choiceContext, creature, loss, isFromCard: false);
    }

    private static async Task AddRegret(Player player)
    {
        if (player.RunState is not ICardScope scope)
        {
            return;
        }

        CardModel regret = scope.CreateCard<Regret>(player);
        scope.AddCard(regret, player);
        await Task.CompletedTask;
    }

    private static async Task RemoveWeightedCard(
        Player player,
        MegaCrit.Sts2.Core.Random.Rng rng)
    {
        var candidates = player.Deck.Cards
            .Where(card => GetWeight(card.Rarity) > 0)
            .ToList();

        if (candidates.Count == 0)
        {
            return;
        }

        int totalWeight = candidates.Sum(card => GetWeight(card.Rarity));
        int roll = rng.NextInt(totalWeight);

        CardModel? selected = null;
        foreach (CardModel card in candidates)
        {
            int weight = GetWeight(card.Rarity);
            if (roll < weight)
            {
                selected = card;
                break;
            }

            roll -= weight;
        }

        if (selected is not null)
        {
            await CardPileCmd.RemoveFromDeck(selected, showPreview: true);
        }
    }

    private static int GetWeight(CardRarity rarity)
    {
        return rarity switch
        {
            CardRarity.Basic => 2,
            CardRarity.Common => 4,
            CardRarity.Uncommon => 4,
            CardRarity.Rare => 4,
            CardRarity.Curse => 1,
            CardRarity.Event => 2,
            _ => 0,
        };
    }
}
