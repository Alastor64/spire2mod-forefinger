using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Forefinger.Game;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Powers;

// 传令员：不可叠加、不可为负的永久 buff。
// 在本场战斗中持续升级所有[指令]：获得时先把此刻战斗里已有的[指令]整批升级，
// 之后任何进入战斗的[指令]（如「眷顾」塞进手牌的随机指令）也各自升级。
// 升级只限本场战斗，战斗结束时由 CombatPrescriptUpgrade 统一还原。
[RegisterPower]
public sealed class ForefingerMessenger : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (ReferenceEquals(power, this))
        {
            CombatPrescriptUpgrade.UpgradeAllFor(Owner);
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (Owner is { } owner && ReferenceEquals(card.Owner?.Creature, owner))
        {
            CombatPrescriptUpgrade.UpgradeCard(card);
        }

        return Task.CompletedTask;
    }

    // 进入战斗的钩子对「这张牌属于谁」还没有定论时可能漏掉，抽到手里再兜一次：
    // 升级过的牌会被 CombatPrescriptUpgrade 直接跳过，所以这里重复触发也没有副作用。
    public override Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        if (Owner is { } owner && ReferenceEquals(card.Owner?.Creature, owner))
        {
            CombatPrescriptUpgrade.UpgradeCard(card);
        }

        return Task.CompletedTask;
    }
}
