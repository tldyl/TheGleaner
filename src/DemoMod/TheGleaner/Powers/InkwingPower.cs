using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Powers;

public class InkwingPower : CustomPowerModel {
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource) {
        if (Owner.HasPower<SlipperyPower>()) {
            PowerModel power = Owner.GetPower<SlipperyPower>();
            power.Removed += OnSlipperyRemoved;
        }
    }

    private void OnSlipperyRemoved() {
        TaskHelper.RunSafely(DoStun());
    }

    private async Task DoStun() {
        await CreatureCmd.Stun(Owner, "LAND_PECK_MOVE");
        await PowerCmd.Remove<InkwingPower>(Owner);
        await CreatureCmd.TriggerAnim(Owner, "IdleLoop", 0.0f);
    }
}
