using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;
using STS2RitsuLib.Patching.Models;
using Forefinger.Characters;
using Forefinger.Relics;

namespace Forefinger.Game;

// 按设计，「业报」是指食指开局即自动获得的事件遗物，位置排在初始遗物「指令加护」之后。
// 它不是初始遗物，所以不能像指令加护那样注册为起始遗物（否则会进入角色选择等起始遗物
// 相关流程）。这里在开局发放完起始遗物后，紧接着把业报补发给食指玩家。
public sealed class KarmaAtRunStartPatch : IPatchMethod
{
    public static string PatchId => "forefinger_karma_at_run_start";
    public static string Description => "Grant the Karma event relic to Forefinger right after its starting relics are populated.";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        PatchTarget.Method<Player>(nameof(Player.PopulateStartingRelics)),
    ];

    public static void Postfix(Player __instance)
    {
        if (__instance.Character is not ForefingerCharacter)
        {
            return;
        }

        if (__instance.GetRelic<ForefingerKarma>() is not null)
        {
            return;
        }

        // 与起始遗物发放流程保持一致：规范化实例克隆为可变实例、记录所在层与已见状态，
        // 追加到遗物列表末尾，因此遗物栏中业报紧跟在「指令加护」之后。
        RelicModel karma = ModelDb.Relic<ForefingerKarma>().ToMutable();
        karma.FloorAddedToDeck = 1;
        SaveManager.Instance.MarkRelicAsSeen(karma);
        __instance.AddRelicInternal(karma, index: -1, silent: false);
    }
}
