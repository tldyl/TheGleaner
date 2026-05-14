using DemoMod.TheGleaner.Cards.GleanerCard;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Commands;

public static class GleanCmd {
    public static async Task Glean(PlayerChoiceContext choiceContext, Player player, CardModel source, int amount) {
        if (player == null || player.Creature?.CombatState == null || amount <= 0) {
            return;
        }

        List<CardModel> cards = [];
        CardModel[] templates = [
            ModelDb.Card<HowlOfWrath>(),
            ModelDb.Card<ShriekOfDread>(),
            ModelDb.Card<DirgeOfFarewell>()
        ];

        for (int i = 0; i < amount; i++) {
            CardModel template = templates[i % templates.Length];
            CardModel created = player.Creature.CombatState.CreateCard(template, player);
            cards.Add(created);
        }

        await ScorePileCmd.AddCards(player.PlayerCombatState, player, cards.ToArray());
    }
}
