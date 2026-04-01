using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace DemoMod.TheGleaner.Cards.TokenCards;

public interface IChoosable {
    public Task OnChosen(PlayerChoiceContext choiceContext, CardPlay cardPlay);
}
