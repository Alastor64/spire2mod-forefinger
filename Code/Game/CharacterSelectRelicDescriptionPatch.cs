using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2RitsuLib.Patching.Models;
using Forefinger.Characters;

namespace Forefinger.Game;

// 原版角色选择界面给起始遗物描述预留的区域很矮，只适合原版那种一句话描述。
// 这里在选中「食指」时临时扩大该区域，切到其他角色时恢复原版布局。
public sealed class CharacterSelectRelicDescriptionPatch : IPatchMethod
{
    public static string PatchId => "forefinger_character_select_relic_description";
    public static string Description => "Enlarge the starting relic description area for the Forefinger character.";
    public static bool IsCritical => false;

    private const float OriginalInfoPanelBottom = 239f;
    private const float ExpandedInfoPanelBottom = 339f;

    private const float OriginalVBoxBottom = 359f;
    private const float ExpandedVBoxBottom = 459f;

    private const float OriginalRelicMinHeight = 100f;
    private const float ExpandedRelicMinHeight = 180f;

    public static ModPatchTarget[] GetTargets() =>
    [
        PatchTarget.Method<NCharacterSelectScreen>(
            nameof(NCharacterSelectScreen.SelectCharacter),
            typeof(NCharacterSelectButton),
            typeof(CharacterModel)),
    ];

    public static void Postfix(
        NCharacterSelectScreen __instance,
        NCharacterSelectButton charSelectButton,
        CharacterModel characterModel)
    {
        if (characterModel is not ForefingerCharacter)
        {
            SetExpanded(__instance, false);
            return;
        }

        SetExpanded(__instance, true);
    }

    private static void SetExpanded(NCharacterSelectScreen screen, bool expanded)
    {
        Control? infoPanel = screen.GetNodeOrNull<Control>("InfoPanel");
        Control? vBox = screen.GetNodeOrNull<Control>("InfoPanel/VBoxContainer");
        Control? relic = screen.GetNodeOrNull<Control>("InfoPanel/VBoxContainer/Relic");
        Control? description = screen.GetNodeOrNull<Control>("InfoPanel/VBoxContainer/Relic/Description");

        if (infoPanel is null || vBox is null || relic is null || description is null)
        {
            return;
        }

        if (expanded)
        {
            infoPanel.OffsetBottom = ExpandedInfoPanelBottom;
            vBox.OffsetBottom = ExpandedVBoxBottom;
            relic.CustomMinimumSize = new Vector2(relic.CustomMinimumSize.X, ExpandedRelicMinHeight);

            description.AnchorBottom = 1f;
            description.OffsetTop = 44f;
            description.OffsetBottom = -4f;
            return;
        }

        infoPanel.OffsetBottom = OriginalInfoPanelBottom;
        vBox.OffsetBottom = OriginalVBoxBottom;
        relic.CustomMinimumSize = new Vector2(relic.CustomMinimumSize.X, OriginalRelicMinHeight);

        description.AnchorBottom = 0f;
        description.OffsetTop = 44f;
        description.OffsetBottom = 101f;
    }
}
