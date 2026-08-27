using STS2RitsuLib.Interop.AutoRegistration;

namespace Forefinger.Keywords;

// 本 mod 需要注册的两个“独一”关键词。
// 它们不是自动行为关键词（CardKeyword），而是卡牌描述里的条件词条，
// 注册后由 RitsuLib 提供悬停提示（hover tip）与本地化标题/描述。
[RegisterOwnedCardKeyword("HandSingleton")]
[RegisterOwnedCardKeyword("DeckSingleton")]
public sealed class ForefingerKeywords
{
    public const string HandSingletonId = "FOREFINGER_KEYWORD_HAND_SINGLETON";
    public const string DeckSingletonId = "FOREFINGER_KEYWORD_DECK_SINGLETON";
}
