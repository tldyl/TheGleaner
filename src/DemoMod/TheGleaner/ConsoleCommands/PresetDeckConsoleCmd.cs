using DemoMod.TheGleaner.Cards.GleanerCard;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.ConsoleCommands;
public class PresetDeckConsoleCmd : AbstractConsoleCmd {
    public override bool DebugOnly => false;
    public override string CmdName => "preset-deck";
    public override string Args => "";
    public override string Description => "将手牌、抽牌堆和弃牌堆替换为一套预设的卡组，录制演示视频用";
    public override bool IsNetworked => false;
    
    public override CmdResult Process(Player? issuingPlayer, string[] args) {
        TaskHelper.RunSafely(replaceDeck(issuingPlayer));
        return new CmdResult(true);
    }

    private async Task replaceDeck(Player? player) {
        await CardPileCmd.RemoveFromCombat(player.PlayerCombatState.DrawPile.Cards.ToList());
        await CardPileCmd.RemoveFromCombat(player.PlayerCombatState.Hand.Cards.ToList());
        await CardPileCmd.RemoveFromCombat(player.PlayerCombatState.DiscardPile.Cards.ToList());
        await CardPileCmd.RemoveFromCombat(player.PlayerCombatState.ExhaustPile.Cards.ToList());

        List<CardModel> presetDeck = [
            ModelDb.Card<StringAndPillar>(),
            ModelDb.Card<StrikeGleaner>(),
            ModelDb.Card<DefendGleaner>(),
            ModelDb.Card<StrikeGleaner>(),
            ModelDb.Card<DefendGleaner>(),
            ModelDb.Card<StrikeGleaner>(),
            
            ModelDb.Card<AutonomousTamb>(),
            ModelDb.Card<BusterArrow>(),
            ModelDb.Card<JeraForm>(),
            ModelDb.Card<DefendGleaner>(),
            ModelDb.Card<StrikeGleaner>(),
            
            ModelDb.Card<SentinelShaft>(),
            ModelDb.Card<SentinelShaft>(),
            ModelDb.Card<StrikeGleaner>(),
            ModelDb.Card<StrikeGleaner>(),
            ModelDb.Card<StrikeGleaner>(),
            
            ModelDb.Card<WindsMuse>(),
            ModelDb.Card<Sonotoxin>(),
            ModelDb.Card<StrikeGleaner>(),
            ModelDb.Card<StrikeGleaner>(),
            ModelDb.Card<DefendGleaner>(),
            
            ModelDb.Card<ClusterStringWeave>(),
            ModelDb.Card<StrikeGleaner>(),
            ModelDb.Card<StrikeGleaner>(),
            ModelDb.Card<StrikeGleaner>(),
            ModelDb.Card<StrikeGleaner>()
        ];

        foreach (CardModel card in presetDeck) {
            CardModel mutableCard = card.ToMutable();
            player.Creature.CombatState.AddCard(mutableCard, player);
            await CardPileCmd.AddGeneratedCardToCombat(mutableCard, PileType.Draw, true);
        }

        await CardPileCmd.Draw(new BlockingPlayerChoiceContext(), 6, player);
    }
}
