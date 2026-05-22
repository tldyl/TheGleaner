using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Powers;

public class ProtectTheKingPower : CustomPowerModel {
    private const string PowerIconPath = "res://TheGleaner/images/powers/demomod-castle_power.png";

    private bool _preventedDamage;

    public override string CustomPackedIconPath => PowerIconPath;
    public override string CustomBigIconPath => PowerIconPath;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override decimal ModifyHpLostAfterOstyLate(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource) {
        _preventedDamage = false;
        if (target != Owner || amount <= 0m || !HasLivingProtector()) {
            return amount;
        }

        _preventedDamage = true;
        return 0m;
    }

    public override Task AfterModifyingHpLostAfterOsty() {
        if (!_preventedDamage) {
            return Task.CompletedTask;
        }

        Flash();
        _preventedDamage = false;
        return Task.CompletedTask;
    }

    private bool HasLivingProtector() {
        return Owner.CombatState.Creatures.Any(creature => creature.IsAlive && creature.IsMonster && creature != Owner);
    }
}
