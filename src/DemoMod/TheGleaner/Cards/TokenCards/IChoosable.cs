using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DemoMod.TheGleaner.Cards.TokenCards;

public interface IChoosable {
    public Task OnChosen(PlayerChoiceContext choiceContext, CardPlay cardPlay);

    public void addVar(DynamicVar dynamicVar);
}
