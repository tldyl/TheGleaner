using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Afflictions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace DemoMod.TheGleaner.Cards.TokenCards;

[Pool(typeof(TokenCardPool))]
public class ArbiterOfStrength : CustomCardModel, IChoosable {
    //public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public ArbiterOfStrength() : base(-1, CardType.Skill, CardRarity.Common, TargetType.None) {
        
    }
    
    public async Task OnChosen(PlayerChoiceContext choiceContext, CardPlay cardPlay, params object[] extraParams) {
        foreach (CardModel card in Owner.PlayerCombatState.AllCards) {
            if (card.Affliction is LightOfLife) {
                card.Affliction.Amount = 3;
            }
        }
    }
}
