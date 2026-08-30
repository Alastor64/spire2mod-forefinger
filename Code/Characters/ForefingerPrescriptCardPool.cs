using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Characters;

// 指令池：与角色池、奖励池分开的卡牌池，卡牌总览中暂用无色图标。
public sealed class ForefingerPrescriptCardPool : TypeListCardPoolModel
{
    public override string Title => "指令";
    public override string EnergyColorName => "colorless";

    public override Color DeckEntryCardColor => ForefingerCharacter.ThemeColor;
    public override Color EnergyOutlineColor => new(0.20f, 0.08f, 0.05f);

    public override bool IsColorless => true;
}
