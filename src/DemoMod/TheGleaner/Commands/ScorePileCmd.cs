using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Cards.GleanerCard;
using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace DemoMod.TheGleaner.Commands;

public static class ScorePileCmd {
    public static bool openingScorePileAndTakeCardsToHand;
    private static readonly Dictionary<PlayerCombatState, int> CombatStartDeckCounts = new();

    public static int GetCapacity(Player player) {
        if (player?.PlayerCombatState == null) {
            return 0;
        }

        PlayerCombatState combatState = player.PlayerCombatState;
        ScorePile scorePile = CustomPiles.GetCustomPile(combatState, CustomEnums.ScorePile) as ScorePile;
        int combatStartDeckCount = player.Deck.Cards.Count;

        if (scorePile != null && scorePile.combatStartDeckCount > 0) {
            combatStartDeckCount = scorePile.combatStartDeckCount;
        } else if (CombatStartDeckCounts.TryGetValue(combatState, out int snapshotDeckCount) && snapshotDeckCount > 0) {
            combatStartDeckCount = snapshotDeckCount;
        }

        return combatStartDeckCount / 3;
    }

    public static void InitializeCapacityFromCurrentDeck(Player player) {
        if (player?.PlayerCombatState == null) {
            return;
        }

        PlayerCombatState combatState = player.PlayerCombatState;
        int deckCount = player.Deck.Cards.Count;
        CombatStartDeckCounts[combatState] = deckCount;

        ScorePile scorePile = CustomPiles.GetCustomPile(combatState, CustomEnums.ScorePile) as ScorePile;
        if (scorePile != null) {
            scorePile.combatStartDeckCount = deckCount;
        }
    }
    
    public static async Task<IEnumerable<CardModel>> ShowScorePileScreen(PlayerCombatState combatState,
        PlayerChoiceContext context,
        Player player, bool freeToTake = false) {
        CardPile pile = CustomPiles.GetCustomPile(combatState, CustomEnums.ScorePile);
        if (pile.Cards.Count == 0) {
            return [];
        }
        CardSelectorPrefs prefs = new CardSelectorPrefs(new LocString("relics", "DEMOMOD-JERA.selectionScreenPrompt"), 0, 2147483647);
        if (!freeToTake) {
            openingScorePileAndTakeCardsToHand = true;
        }
        IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromSimpleGrid(context, pile.Cards, player, prefs);
        if (!freeToTake) {
            openingScorePileAndTakeCardsToHand = false;
        }
        return selectedCards;
    }

    public static async Task AddCards(PlayerCombatState combatState, Player player, params CardModel[] cards) {
        ScorePile pile = GetOrCreateScorePile(combatState);
        if (pile.combatStartDeckCount <= 0) {
            if (CombatStartDeckCounts.TryGetValue(combatState, out int snapshotDeckCount) && snapshotDeckCount > 0) {
                pile.combatStartDeckCount = snapshotDeckCount;
            } else {
                pile.combatStartDeckCount = player.Deck.Cards.Count;
            }
        }

        int capacity = GetCapacity(player);
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
                await Hook.AfterCardChangedPiles(player.RunState, player.Creature.CombatState, bottomCard, CustomEnums.ScorePile, bottomCard);
            } else {
                pile.AddInternal(card, 0);
            }
            if (card is IDissonanceCard dissonanceCard) {
                dissonanceCard.OnEnterScorePile(combatState, player);
            }
        }
        if (cards.Length > 0) {
            pile.cardsAddedToScoreThisTurn = true;
        }
        if (pile.Cards.Count > 0 && !combatState.Hand.Cards.Any(c => c is ScoreEntryCard)) {
            CardModel scoreEntryCard = ModelDb.Card<ScoreEntryCard>().ToMutable();
            player.Creature.CombatState.AddCard(scoreEntryCard, player);
            await CardPileCmd.AddGeneratedCardToCombat(scoreEntryCard, PileType.Hand, true);
        }
    }

    public static void RemoveCardsFromScoreOnly(PlayerCombatState combatState, Player player, IEnumerable<CardModel> cards) {
        ScorePile pile = GetOrCreateScorePile(combatState);
        foreach (CardModel card in cards) {
            pile.RemoveInternal(card);
        }
    }
    
    public static async Task Glean(Player player, PlayerChoiceContext choiceContext, decimal baseValue, CardModel cardSource) {
        CardSelectorPrefs prefs = new CardSelectorPrefs(new LocString("cards", "DEMOMOD-WINDS_MUSE.selectionScreenPrompt"), 0, (int) baseValue);
        IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromHand(choiceContext, player, prefs, _ => true, cardSource);
        await AddCards(player.PlayerCombatState, player, selectedCards.ToArray());
        if (selectedCards.Count() < baseValue) {
            int amount = (int) baseValue - selectedCards.Count();
            CardPile drawPile = PileType.Draw.GetPile(player);
            List<CardModel> cards = [];
            for (int i = 0; i < amount; i++) {
                await CardPileCmd.ShuffleIfNecessary(choiceContext, player);
                CardModel cardModel = drawPile.Cards.FirstOrDefault();
                if (cardModel != null) {
                    cardModel.RemoveFromCurrentPile();
                    cards.Add(cardModel);
                } else {
                    break;
                }
            }
            await AddCards(player.PlayerCombatState, player, cards.ToArray());
        }
    }

    public static ScorePile GetOrCreateScorePile(PlayerCombatState combatState) {
        if (!CustomPiles.CustomPileProviders.ContainsKey(CustomEnums.ScorePile)) {
            CustomPiles.CustomPileProviders[CustomEnums.ScorePile] = () => new ScorePile();
        }

        ScorePile pile = CustomPiles.GetCustomPile(combatState, CustomEnums.ScorePile) as ScorePile;
        if (pile != null) {
            return pile;
        }

        Dictionary<PileType, CustomPile> dictionary = CustomPiles.Piles.Get(combatState);
        if (dictionary == null) {
            dictionary = new Dictionary<PileType, CustomPile>();
            CustomPiles.Piles.Set(combatState, dictionary);
        }

        pile = new ScorePile();
        dictionary[CustomEnums.ScorePile] = pile;
        return pile;
    }
}
