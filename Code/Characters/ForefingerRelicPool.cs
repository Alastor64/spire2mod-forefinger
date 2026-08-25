using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Characters;

public sealed class ForefingerRelicPool : TypeListRelicPoolModel
{
    public override string EnergyColorName => "Forefinger";
    public override Color LabOutlineColor => ForefingerCharacter.ThemeColor;
}

