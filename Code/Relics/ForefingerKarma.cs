using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using Forefinger.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Relics;

// 业报：跨战斗保留的计数器遗物。每层业报由 [指令期限] 倒计时归零产生。
// 计数器达到 20 时失去 10 点生命上限（最多降至 1）并减 20；没有上限，因此 60 会连续触发三次。
// 按设计，业报在开局时自动获得、排在初始遗物「指令加护」之后，但它是事件遗物而不是初始遗物，
// 所以不注册为起始遗物，由 KarmaAtRunStartPatch 在开局发放完成后补上。
[RegisterRelic(typeof(ForefingerRelicPool))]
public sealed class ForefingerKarma : ModRelicTemplate
{
    private const int Threshold = 20;

    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool ShowCounter => true;
    public override int DisplayAmount => DynamicVars["Counter"].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Counter", 0m),
    ];

    // 不进入普通遗物池，只作为食指开局自动获得的事件遗物出现。
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
            // 先触发效果，再让计数器减 20（与设计描述一致）。
            await LoseMaxHp(choiceContext, player);
            counter -= Threshold;
            karma.SetCounter(counter);
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
}
