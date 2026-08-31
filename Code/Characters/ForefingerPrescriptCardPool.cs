using Godot;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Characters;

// 指令池：与角色池、奖励池分开的卡牌池，卡牌总览中暂用无色图标。
// 必须注册为共享卡池，否则卡牌池解析（CardModel.Pool）会落到测试用的 MockCardPool
// 并在生成卡牌节点时抛异常，导致「下回合执行」无法把指令牌加入手牌。
[RegisterSharedCardPool]
public sealed class ForefingerPrescriptCardPool : TypeListCardPoolModel
{
    public override string Title => "指令";
    public override string EnergyColorName => "colorless";

    public override Color DeckEntryCardColor => ForefingerCharacter.ThemeColor;
    public override Color EnergyOutlineColor => new(0.20f, 0.08f, 0.05f);

    public override bool IsColorless => true;
}
