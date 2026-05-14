using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Powers;

public class SforzandoPower : CustomPowerModel {
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterAttack(AttackCommand command) {
        if (command.Attacker != Owner) {
            return;
        }
        int blockSum = command.Results.Sum(damageResult => damageResult.BlockedDamage + damageResult.UnblockedDamage + damageResult.OverkillDamage);
        Flash();
        await CreatureCmd.GainBlock(Owner, blockSum, ValueProp.Unpowered, null);
        await PowerCmd.Decrement(this);
    }
    
    public override async Task AfterBlockCleared(Creature creature) {
        if (creature != Owner) {
            return;
        }
        await PowerCmd.Remove(this);
    }
}
