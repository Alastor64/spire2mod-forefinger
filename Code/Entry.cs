using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;
using Forefinger.Game;

namespace Forefinger;

[ModInitializer(nameof(Initialize))]
public static class Entry
{
    public const string ModId = "Forefinger";
    public const string ResPath = $"res://{ModId}";

    public static Logger Logger { get; private set; } = null!;

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();

        Logger = RitsuLibFramework.CreateLogger(ModId);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        var characterSelectPatcher = RitsuLibFramework.CreatePatcher(ModId, "character_select_ui");
        characterSelectPatcher.RegisterPatch<CharacterSelectRelicDescriptionPatch>();
        characterSelectPatcher.PatchAll();

        var cardRewardPatcher = RitsuLibFramework.CreatePatcher(ModId, "card_reward");
        cardRewardPatcher.RegisterPatch<CardRewardBasicFallbackPatch>();
        cardRewardPatcher.PatchAll();

        var handGlowPatcher = RitsuLibFramework.CreatePatcher(ModId, "hand_glow");
        handGlowPatcher.RegisterPatch<HandGlowRefreshPatch>();
        handGlowPatcher.PatchAll();

        var deadlineCostPatcher = RitsuLibFramework.CreatePatcher(ModId, "deadline_cost");
        deadlineCostPatcher.RegisterPatch<DeadlineCostPatch>();
        deadlineCostPatcher.PatchAll();

        CombatManager.Instance.CombatEnded += CombatEnchantTracker.OnCombatEnded;

        Logger.Info("Forefinger initialized.");
    }
}
