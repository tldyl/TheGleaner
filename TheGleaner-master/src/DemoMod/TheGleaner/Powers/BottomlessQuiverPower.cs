using BaseLib.Abstracts;
using DemoMod.TheGleaner.Enums;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace DemoMod.TheGleaner.Powers;

public class BottomlessQuiverPower : CustomPowerModel {
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private bool activated = true;
    
    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        CombatState combatState) {
        if (side == Owner.Side) {
            activated = true;
        }
    }
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
        if ((cardPlay.Card.Tags.Contains(CardTag.Strike) || cardPlay.Card.Tags.Contains(CustomEnums.Arrow)) && activated && Owner.IsPlayer) {
            await PlayerCmd.GainEnergy(Amount, Owner.Player);
            activated = false;
        }
    }
}
