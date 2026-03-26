using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Cards.GleanerCard;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace DemoMod.TheGleaner.Commands;

public static class ScorePileCmd {
    public static bool openingScorePile;
    
    public static async Task<IEnumerable<CardModel>> ShowScorePileScreen(PlayerCombatState combatState,
        PlayerChoiceContext context,
        Player player) {
        CardPile pile = CustomPiles.GetCustomPile(combatState, CustomEnums.ScorePile);
        CardSelectorPrefs prefs = new CardSelectorPrefs(new LocString("relics", "DEMOMOD-JERA.selectionScreenPrompt"), 0, 2147483647);
        openingScorePile = true;
        IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromSimpleGrid(context, pile.Cards, player, prefs);
        openingScorePile = false;
        return selectedCards;
    }

    public static async Task AddCards(PlayerCombatState combatState, Player player, params CardModel[] cards) {
        CardPile pile = CustomPiles.GetCustomPile(combatState, CustomEnums.ScorePile);
        if (pile == null) {
            Dictionary<PileType, CustomPile> dictionary = CustomPiles.Piles.Get(combatState);
            if (dictionary == null) {
                dictionary = new Dictionary<PileType, CustomPile>();
                CustomPiles.Piles.Set(combatState, dictionary);
            }
            pile = new ScorePile();
            dictionary.Add(CustomEnums.ScorePile, (CustomPile) pile);
        }
        int capacity = player.Deck.Cards.Count / 3;
        foreach (CardModel card in cards) {
            card.RemoveFromCurrentPile();
            if (NRun.Instance.CombatRoom.Ui.Hand.GetCardHolder(card) != null) {
                NRun.Instance.CombatRoom.Ui.Hand.Remove(card);
            }
            if (pile.Cards.Count >= capacity) {
                pile.AddInternal(card, 0);
                CardModel bottomCard = pile.Cards.Last();
                pile.RemoveInternal(bottomCard);
                await CardPileCmd.Add(bottomCard, PileType.Discard);
                PileType.Discard.GetPile(player).InvokeCardAddFinished();
            } else {
                pile.AddInternal(card, 0);
            }
            if (card is IDissonanceCard dissonanceCard) {
                dissonanceCard.OnEnterScorePile(combatState, player);
            }
        }
        if (pile.Cards.Count > 0 && !combatState.Hand.Cards.Any(c => c is ScoreEntryCard)) {
            CardModel scoreEntryCard = ModelDb.Card<ScoreEntryCard>().ToMutable();
            player.Creature.CombatState.AddCard(scoreEntryCard, player);
            await CardPileCmd.AddGeneratedCardToCombat(scoreEntryCard, PileType.Hand, true);
        }
    }
}
