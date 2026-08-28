using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Forefinger.Characters;
using Forefinger.Keywords;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Cards;

// 指令所向：造成 6 点伤害；若「手牌独一」，额外造成 1 次伤害。
// 升级后伤害 6 → 9。
[RegisterCard(typeof(ForefingerCardPool))]
[RegisterCharacterStarterCard(typeof(ForefingerCharacter), 1)]
public sealed class ForefingerToWhereThePrescriptPoints : ModCardTemplate
{
    private const int BaseDamage = 6;
    private const int UpgradeDamageBonus = 3;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordExtensions.GetModCardKeyword(ForefingerKeywords.HandSingletonId),
    ];

    // 手牌独一满足时发光（金色），提示玩家此时打出能触发额外伤害。
    // 与「信仰之刃」一致：打出时这张牌会进入运行区，判定时排除自己。
    protected override bool ShouldGlowGoldInternal =>
        CardIdentity.IsHandSingletonExcluding(this);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
    ];

    public ForefingerToWhereThePrescriptPoints()
        : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var hitCount = CardIdentity.IsHandSingleton(Owner) ? 2 : 1;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitCount(hitCount)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(UpgradeDamageBonus);
    }
}
