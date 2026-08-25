using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Characters;

public sealed class ForefingerCardPool : TypeListCardPoolModel
{
    public override string Title => "Forefinger";
    public override string EnergyColorName => "colorless";

    public override Color DeckEntryCardColor => ForefingerCharacter.ThemeColor;
    public override Color EnergyOutlineColor => new(0.36078432f, 0.32941177f, 0.2509804f);

    public override bool IsColorless => false;
}
