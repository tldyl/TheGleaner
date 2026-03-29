using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Powers;

public class NoLeaveToWitherPower : CustomPowerModel {
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    private Creature target;
    private Decimal damageAmount;
    
    public override Decimal ModifyHpLostAfterOstyLate(
        Creature target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource) {
        this.target = target;
        damageAmount = amount;
        return target != Owner ? amount : 0M;
    }

    public override async Task AfterModifyingHpLostAfterOsty() {
        if (target == Owner && damageAmount > 0) {
            await PowerCmd.Apply<DoomPower>(Owner, damageAmount, Owner, null);
        }
    }

    public override async Task AfterBlockCleared(Creature creature) {
        if (creature != Owner) {
            return;
        }
        await PowerCmd.Remove(this);
    }
}
