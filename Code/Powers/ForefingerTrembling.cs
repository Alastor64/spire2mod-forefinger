using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Forefinger.Powers;

// 战栗：下回合开始（抽牌前）把全部层数转为等量「易伤」的延迟效果。
// 施加的是原版减益，所以它自己是 debuff。
[RegisterPower]
public sealed class ForefingerTrembling : ForefingerNextTurnPower<VulnerablePower>;
