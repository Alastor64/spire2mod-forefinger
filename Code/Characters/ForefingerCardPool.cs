using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Characters;

public sealed class ForefingerCardPool : TypeListCardPoolModel
{
    public override string Title => "Forefinger";
    public override string EnergyColorName => "Forefinger";

    public override Color DeckEntryCardColor => ForefingerCharacter.ThemeColor;
    public override Color EnergyOutlineColor => new(0.20f, 0.08f, 0.05f);

    public override bool IsColorless => false;
}

