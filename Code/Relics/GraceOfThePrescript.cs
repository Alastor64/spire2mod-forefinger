using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Forefinger.Characters;
using STS2RitsuLib;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Forefinger.Relics;

[RegisterRelic(typeof(ForefingerRelicPool))]
[RegisterCharacterStarterRelic(typeof(ForefingerCharacter))]
public sealed class GraceOfThePrescript : ModRelicTemplate
{
    private const int EffectCount = 4;

    public override RelicRarity Rarity => RelicRarity.Starter;

    // 遗物数值统一放在这里，本地化里的 {VigorPower}/{Block}/{Cards}/{Energy} 会从这里取值，
    // 生效逻辑也读取同一个来源，避免数值写两处。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VigorPower>(4m),
        new BlockVar(4m, ValueProp.Move),
        new CardsVar(2),
        new EnergyVar(1),
    ];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        Rng rng = RitsuLibFramework.GetModPlayerRng(player, Entry.ModId, nameof(GraceOfThePrescript));

        int effectIndex = rng.NextInt(EffectCount);
        switch (effectIndex)
        {
            case 0:
                await PowerCmd.Apply<VigorPower>(
                    choiceContext, player.Creature, DynamicVars["VigorPower"].BaseValue, player.Creature, null, false);
                break;
            case 1:
                await CreatureCmd.GainBlock(player.Creature, DynamicVars.Block, null);
                break;
            case 2:
                await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, player);
                break;
            case 3:
                await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, player);
                break;
            default:
                Entry.Logger.Error($"指令加护收到了无效的效果编号：{effectIndex}");
                break;
        }
    }
}
