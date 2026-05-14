using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Powers;

public class RendezvousWithDoomPower : CustomPowerModel {
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    private bool activated = true;

    public override async Task AfterBlockCleared(Creature creature) {
        if (creature != Owner) {
            return;
        }
        activated = true;
        await PowerCmd.Decrement(this);
        if (Amount <= 0) {
            await CreatureCmd.Kill(Owner);
        }
    }
    
    public override async Task BeforeApplied(
        Creature target,
        Decimal amount,
        Creature? applier,
        CardModel? cardSource) {
        if (!target.IsPlayer) {
            return;
        }
        target.Player.PlayerCombatState.EnergyChanged += onEnergyChanged;
        Removed += unsubscribeEnergyChanged;
    }

    public override async Task AfterRemoved(Creature target) {
        if (!target.IsPlayer) {
            return;
        }
        target.Player.PlayerCombatState.EnergyChanged -= onEnergyChanged;
    }

    public override async Task AfterCombatVictory(CombatRoom room) {
        if (!Owner.IsPlayer) {
            return;
        }
        Owner.Player.PlayerCombatState.EnergyChanged -= onEnergyChanged;
    }

    public void unsubscribeEnergyChanged() {
        Owner.Player.PlayerCombatState.EnergyChanged -= onEnergyChanged;
    }
    
    private void onEnergyChanged(int before, int after) {
        if (after <= 0 && activated) {
            activated = false;
            TaskHelper.RunSafely(PlayerCmd.GainEnergy(Owner.Player.PlayerCombatState.MaxEnergy, Owner.Player));
        }
    }
}
