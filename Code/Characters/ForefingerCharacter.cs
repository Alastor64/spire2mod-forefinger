using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;

namespace Forefinger.Characters;

[RegisterCharacter]
public sealed class ForefingerCharacter
    : ModCharacterTemplate<ForefingerCardPool, ForefingerRelicPool, ForefingerPotionPool>
{
    public static readonly Color ThemeColor = new(0.80f, 0.35f, 0.20f);

    public override Color NameColor => ThemeColor;
    public override Color EnergyLabelOutlineColor => new(0.20f, 0.08f, 0.05f);
    public override Color MapDrawingColor => ThemeColor;

    public override CharacterGender Gender => CharacterGender.Neutral;

    public override int StartingHp => 75;
    public override int StartingGold => 99;

    // 不提供贴图/场景，缺失资源一律从占位角色（铁甲战士）回退。
    public override string? PlaceholderCharacterId => "ironclad";

    // 不参与原版 epoch/timeline 解锁流程，可直接从角色选择界面开始游戏。
    public override bool RequiresEpochAndTimeline => false;

    public override float AttackAnimDelay => 0f;
    public override float CastAnimDelay => 0f;

    public override List<string> GetArchitectAttackVfx()
    {
        return
        [
            "vfx/vfx_attack_blunt",
            "vfx/vfx_heavy_blunt",
            "vfx/vfx_attack_slash"
        ];
    }
}

