using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using STS2RitsuLib.Patching.Models;

namespace Forefinger.Game;

// 食指目前只实现了「基础」稀有度的打击/防御，卡池里还没有普通/罕见/稀有卡。
// 原版卡牌奖励会先在卡池现有稀有度里挑一个，再生成对应稀有度的卡；如果池子里
// 一个可奖励稀有度都没有，CardFactory 会抛异常，导致整个战后奖励窗口（卡牌+金币）
// 一起消失。
// 这里临时把这种情况回退到「基础」稀有度，保证奖励窗口能正常出现；等后续补齐
// 普通/罕见/稀有卡后，此补丁会因卡池已存在可奖励稀有度而自动变成空操作。
public sealed class CardRewardBasicFallbackPatch : IPatchMethod
{
    public static string PatchId => "forefinger_card_reward_basic_fallback";
    public static string Description => "Fall back to Basic card rewards while the Forefinger pool has no Common/Uncommon/Rare cards.";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        PatchTarget.Method(typeof(CardFactory), nameof(CardFactory.GetNextAllowedRarity)),
    ];

    public static bool Prefix(ref CardRarity __result, CardRarity rarity, Func<CardRarity, bool> isAllowed)
    {
        if (isAllowed(CardRarity.Common) || isAllowed(CardRarity.Uncommon) || isAllowed(CardRarity.Rare))
        {
            return true;
        }

        if (isAllowed(CardRarity.Basic))
        {
            __result = CardRarity.Basic;
            return false;
        }

        return true;
    }
}
