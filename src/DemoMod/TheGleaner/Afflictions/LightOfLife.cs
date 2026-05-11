using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Afflictions;

public class LightOfLife : AfflictionModel {
    public override bool HasExtraCardText => true;
    private List<CardModel> siblingCards = [];

    private void RefreshSibling(AbstractModel _this) {
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

    protected override void AfterCloned() {
        base.AfterCloned();
        ExecutionFinished += RefreshSibling;
    } 
}
