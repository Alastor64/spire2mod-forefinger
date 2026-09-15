using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Forefinger.Powers;

// 荒诞：下回合开始（抽牌前）把全部层数转为等量「本回合结束前获得」的临时敏捷的延迟效果。
// 照抄原版「螺旋镖」：施加正值的 HelicalDartPower，即立刻获得与层数等量的敏捷，
// 持续至该回合结束时自动解除；它自己是增益，所以荒诞也是 buff。
[RegisterPower]
public sealed class ForefingerAbsurdity : ForefingerNextTurnPower<HelicalDartPower>;
