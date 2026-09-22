using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Forefinger.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Cards;

// 传令员：稀有能力的牌，1 费，对象自身。
// 打出后获得 2 层 buff「心理暗示」（手牌上限 +2）与 1 层 buff「传令员」；
// 传令员在本场战斗中升级所有[指令]（见 Powers.ForefingerMessenger）。
// 能力牌天然被[消耗]，不必写进描述；升级后获得[固有]。
[RegisterCard(typeof(ForefingerCardPool))]
public sealed class ForefingerMessenger : ModCardTemplate
{
    private const decimal SelfSuggestionStacks = 2m;

    public ForefingerMessenger()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature is not { } creature)
        {
            return;
        }

        await PowerCmd.Apply<Forefinger.Powers.ForefingerSelfSuggestion>(
            choiceContext, creature, SelfSuggestionStacks, creature, this, silent: false);

        await PowerCmd.Apply<Forefinger.Powers.ForefingerMessenger>(
            choiceContext, creature, 1m, creature, this, silent: false);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
