using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace DemoMod.TheGleaner.Cards.TokenCards;

[Pool(typeof(TokenCardPool))]
public class Shuffle : CustomCardModel, IChoosable {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    public override bool CanBeGeneratedInCombat => false;
    public override int MaxUpgradeLevel => 0;
    
    public Shuffle() : base(-1, CardType.Skill, CardRarity.Token, TargetType.None) {
        
    }

    public async Task OnChosen(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        List<CardModel> cardsInDrawPile = cardPlay.Card.Owner.PlayerCombatState.DrawPile.Cards.ToList();
        await CardPileCmd.Add(cardsInDrawPile, PileType.Discard, CardPilePosition.Bottom, cardPlay.Card);
    }
}
