using BaseLib.Abstracts;
using DemoMod.TheGleaner.Enums;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Powers;

public class MelodyAttunedPower : CustomPowerModel {
    public override string CustomPackedIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string CustomBigIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("LeftAmount", 0)];
    public override int DisplayAmount => DynamicVars["LeftAmount"].IntValue;

    public override async Task BeforeApplied(
        Creature target,
        Decimal amount,
        Creature? applier,
        CardModel? cardSource) {
        DynamicVars["LeftAmount"].BaseValue = amount;
        InvokeDisplayAmountChanged();
    }

    public override async Task AfterModifyingPowerAmountReceived(PowerModel power) {
        if (power == this) {
            DynamicVars["LeftAmount"].BaseValue++;
            InvokeDisplayAmountChanged();
        }
    }
    
    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player) {
        if (player.Creature != Owner) {
            return;
        }
        DynamicVars["LeftAmount"].BaseValue = Amount;
        InvokeDisplayAmountChanged();
    }
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
        if (cardPlay.Card.Owner.Creature == Owner && cardPlay.Card.Keywords.Contains(CustomEnums.Resonance) && cardPlay.Resources.EnergySpent > 0 && DynamicVars["LeftAmount"].BaseValue > 0) {
            Flash();
            DynamicVars["LeftAmount"].BaseValue--;
            InvokeDisplayAmountChanged();
            await PlayerCmd.GainEnergy(cardPlay.Resources.EnergySpent, cardPlay.Card.Owner);
        }
    }
}
