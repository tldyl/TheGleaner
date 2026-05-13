using BaseLib.Abstracts;
using DemoMod.TheGleaner.Afflictions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Powers;

public class LightOfLifePower : CustomPowerModel {
    public override string CustomPackedIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string CustomBigIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
        AfflictionModel affliction = cardPlay.Card.Affliction;
        if (affliction is LightOfLife) {
            Flash();
            foreach (Creature creature in Owner.CombatState.Enemies) {
                if (creature.HasPower<DeclarationOfTheEndPower>()) {
                    await PowerCmd.Apply<DemoTempStrengthPower>(creature, -Amount, Owner, null);
                    await PowerCmd.Apply<DemoTempStrengthPower>(Owner, Amount, Owner, null);
                }
            }
        }
    }
}
