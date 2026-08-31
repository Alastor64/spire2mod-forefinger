using STS2RitsuLib.Interop.AutoRegistration;

namespace Forefinger.Keywords;

// 本 mod 需要注册的显示型关键词。
// 它们不是自动行为关键词（CardKeyword），而是卡牌描述里的条件词条或说明词条，
// 注册后由 RitsuLib 提供悬停提示（hover tip）与本地化标题/描述。
[RegisterOwnedCardKeyword("HandSingleton")]
[RegisterOwnedCardKeyword("DeckSingleton")]
[RegisterOwnedCardKeyword("Prescript")]
[RegisterOwnedCardKeyword("Deadline")]
[RegisterOwnedCardKeyword("Karma")]
[RegisterOwnedCardKeyword("Unlocking")]
[RegisterOwnedCardKeyword("BladeUnlocked")]
public sealed class ForefingerKeywords
{
    public const string HandSingletonId = "FOREFINGER_KEYWORD_HAND_SINGLETON";
    public const string DeckSingletonId = "FOREFINGER_KEYWORD_DECK_SINGLETON";
    public const string PrescriptId = "FOREFINGER_KEYWORD_PRESCRIPT";
    public const string DeadlineId = "FOREFINGER_KEYWORD_DEADLINE";
    public const string KarmaId = "FOREFINGER_KEYWORD_KARMA";
    public const string UnlockingId = "FOREFINGER_KEYWORD_UNLOCKING";
    public const string BladeUnlockedId = "FOREFINGER_KEYWORD_BLADE_UNLOCKED";
}
