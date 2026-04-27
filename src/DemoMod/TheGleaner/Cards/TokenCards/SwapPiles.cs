using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace DemoMod.TheGleaner.Cards.TokenCards;

[Pool(typeof(TokenCardPool))]
public class SwapPiles : CustomCardModel, IChoosable {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    public override bool CanBeGeneratedInCombat => false;
    public override int MaxUpgradeLevel => 0;

    public SwapPiles() : base(-1, CardType.Skill, CardRarity.Token, TargetType.None) {
        
    }

    public async Task OnChosen(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        List<CardModel> cardsInDrawPile = cardPlay.Card.Owner.PlayerCombatState.DrawPile.Cards.ToList();
        List<CardModel> cardsInDiscardPile = cardPlay.Card.Owner.PlayerCombatState.DiscardPile.Cards.ToList();
        await CardPileCmd.Add(cardsInDrawPile, PileType.Discard, CardPilePosition.Bottom, cardPlay.Card);
        await CardPileCmd.Add(cardsInDiscardPile, PileType.Draw, CardPilePosition.Bottom, cardPlay.Card);
    }
    
    public void addVar(DynamicVar dynamicVar) {
        
    }
}
