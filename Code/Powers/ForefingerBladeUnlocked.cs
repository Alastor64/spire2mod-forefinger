using MegaCrit.Sts2.Core.Entities.Powers;
using Forefinger.Keywords;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Powers;

// 剑刃解放：不可叠加的标记型状态。实际力量/敏捷增益由「解禁中」跨越阈值时结算。
[RegisterPower]
public sealed class ForefingerBladeUnlocked : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    protected override IEnumerable<string> RegisteredKeywordIds => [ForefingerKeywords.BladeUnlockedId];
}
