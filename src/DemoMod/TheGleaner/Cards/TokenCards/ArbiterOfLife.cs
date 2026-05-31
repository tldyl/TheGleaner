using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace DemoMod.TheGleaner.Cards.TokenCards;

[Pool(typeof(TokenCardPool))]
public class ArbiterOfLife : CustomCardModel {
    //public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Ethereal,
        CardKeyword.Exhaust
    ];

    public override int MaxUpgradeLevel => 0;

    private List<CardModel>? optionCards;
    
    private List<CardModel> OptionCards {
        get {
            if (optionCards == null) {
                optionCards = [ModelDb.Card<ArbiterOfStrength>().ToMutable(), ModelDb.Card<ArbiterOfFocus>().ToMutable(), ModelDb.Card<ArbiterOfHeal>().ToMutable()];
                foreach (CardModel card in optionCards) {
                    card.Owner = Owner;
                }
            }
            return optionCards;
        }
    }

    public ArbiterOfLife() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        CardModel chosenCard = await CardSelectCmd.FromChooseACardScreen(choiceContext, OptionCards, Owner);
        if (chosenCard != null) {
            await ((IChoosable)chosenCard).OnChosen(choiceContext, cardPlay);
        }
    }
}
