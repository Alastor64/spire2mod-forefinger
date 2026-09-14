using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Powers;

// 心理暗示：可叠加、可为负的永久 buff。
// 层数直接参与持有者的手牌上限计算（见 Forefinger.Game.ForefingerHandLimit）。
// 负层时按原版「力量」「敏捷」的做法，由 PowerModel.GetTypeForAmount 自动当成 debuff 显示（数字标红），
// 并且 AllowNegative 的 power 在层数恰好为 0 时整条移除，所以正负层共用同一条 buff。
[RegisterPower]
public sealed class ForefingerSelfSuggestion : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => true;

    // 层数为负时描述改成「手牌上限减少{n}」，n 取层数绝对值（原版 abs() 格式化器）。
    public override LocString Description =>
        Loc(Amount < 0 ? ".descriptionDown" : ".description");

    protected override string SmartDescriptionLocKey =>
        Id.Entry + (Amount < 0 ? ".smartDescriptionDown" : ".smartDescription");

    private LocString Loc(string suffix) => new("powers", Id.Entry + suffix);
}
