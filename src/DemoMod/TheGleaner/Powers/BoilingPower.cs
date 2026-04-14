using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace DemoMod.TheGleaner.Powers;

public class BoilingPower : CustomPowerModel {
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side) {
        if (side != Owner.Side) {
            return;
        }
        await PowerCmd.Remove(this);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
        if (cardPlay.Card.Owner != Owner.Player) {
            return;
        }
        if (cardPlay.Card.Type == CardType.Attack) {
            Flash();
            PlayerCmd.EndTurn(Owner.Player, false);
        }
    }
}
