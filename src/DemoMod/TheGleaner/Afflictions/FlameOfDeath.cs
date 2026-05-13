using DemoMod.TheGleaner.Hooks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Afflictions;

public class FlameOfDeath : AfflictionModel, IAfterTakeCardsFromScore {
    public override bool HasExtraCardText => true;
    private List<CardModel> siblingCards = [];

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
        RefreshSibling();
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw) {
        RefreshSibling();
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card) {
        RefreshSibling();
    }

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal) {
        RefreshSibling();
    }
    
    public async Task AfterTakeCardsFromScore(CardModel card) {
        RefreshSibling();
    }
    
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source) {
        if (card.Pile?.Type == PileType.Hand) {
            RefreshSibling();
        }
    }

    private void RefreshSibling() {
        if (Card.Pile?.Type == PileType.Hand) {
            siblingCards.Clear();
            List<CardModel> hand = Card.Pile.Cards.ToList();
            int index = hand.IndexOf(Card);
            if (index > 0 && index < Card.Pile.Cards.Count - 1) {
                siblingCards.Add(hand[index - 1]);
                siblingCards.Add(hand[index + 1]);
            } else if (index == 0) {
                if (Card.Pile.Cards.Count > 1) {
                    siblingCards.Add(hand[1]);
                }
            } else {
                siblingCards.Add(hand[index - 1]);
            }
        }
    }
    
    protected override void DeepCloneFields() {
        List<CardModel> _siblingCards = [];
        _siblingCards.AddRange(siblingCards);
        siblingCards = _siblingCards;
    }
}
