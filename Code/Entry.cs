using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;
using Forefinger.Characters;
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

        // 指令池需要在百科-卡牌总览中显示；图标暂用游戏本体的无色能量图标。
        ModContentRegistry.For(ModId)
            .RegisterCardLibraryCompendiumSharedPoolFilter<ForefingerPrescriptCardPool>(
                "forefinger_prescript_pool",
                "res://images/atlases/ui_atlas.sprites/card/energy_colorless.tres");

        var characterSelectPatcher = RitsuLibFramework.CreatePatcher(ModId, "character_select_ui");
        characterSelectPatcher.RegisterPatch<CharacterSelectRelicDescriptionPatch>();
        characterSelectPatcher.PatchAll();

        var cardRewardPatcher = RitsuLibFramework.CreatePatcher(ModId, "card_reward");
        cardRewardPatcher.RegisterPatch<CardRewardBasicFallbackPatch>();
        cardRewardPatcher.PatchAll();

        var handGlowPatcher = RitsuLibFramework.CreatePatcher(ModId, "hand_glow");
        handGlowPatcher.RegisterPatch<HandGlowRefreshPatch>();
        handGlowPatcher.PatchAll();

        // 「心理暗示」会改手牌上限，并让手牌已满时费用为 0：前者要改原版静态常量，
        // 后者要在手牌内容变化后重刷费用显示。
        var handLimitPatcher = RitsuLibFramework.CreatePatcher(ModId, "hand_limit");
        handLimitPatcher.RegisterPatch<HandLimitPatch>();
        handLimitPatcher.RegisterPatch<HandLimitScopePatch>();
        handLimitPatcher.PatchAll();

        var handVisualPatcher = RitsuLibFramework.CreatePatcher(ModId, "hand_visual_refresh");
        handVisualPatcher.RegisterPatch<HandVisualRefreshPatch>();
        handVisualPatcher.PatchAll();

        var deadlineCostPatcher = RitsuLibFramework.CreatePatcher(ModId, "deadline_cost");
        deadlineCostPatcher.RegisterPatch<DeadlineCostPatch>();
        deadlineCostPatcher.PatchAll();

        var karmaPatcher = RitsuLibFramework.CreatePatcher(ModId, "run_start_relic");
        karmaPatcher.RegisterPatch<KarmaAtRunStartPatch>();
        karmaPatcher.PatchAll();

        CombatManager.Instance.CombatEnded += CombatEnchantTracker.OnCombatEnded;
        CombatManager.Instance.CombatEnded += CombatPrescriptUpgrade.OnCombatEnded;

        Logger.Info("Forefinger initialized.");
    }
}
