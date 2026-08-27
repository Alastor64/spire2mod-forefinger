using System.Reflection;
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

        Logger.Info("Forefinger initialized.");
    }
}
