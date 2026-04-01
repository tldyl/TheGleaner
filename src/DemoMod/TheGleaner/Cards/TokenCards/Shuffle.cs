using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace DemoMod.TheGleaner.Cards.TokenCards;

[Pool(typeof(TokenCardPool))]
public class Shuffle : CustomCardModel, IChoosable {
    public override bool CanBeGeneratedInCombat => false;
    public override int MaxUpgradeLevel => 0;
    
    public Shuffle() : base(-1, CardType.Skill, CardRarity.Token, TargetType.None) {
        
    }

    public async Task OnChosen(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await CardPileCmd.Shuffle(choiceContext, cardPlay.Card.Owner);
    }
}
