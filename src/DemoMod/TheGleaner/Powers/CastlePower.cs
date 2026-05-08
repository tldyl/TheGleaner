using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Powers;

public class CastlePower : CustomPowerModel {
    public override string CustomPackedIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string CustomBigIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState) {
        if (side == Owner.Side) {
            return;
        }

        List<Creature> targets = combatState.Creatures
            .Where(creature => creature.IsAlive && creature.IsMonster && creature != Owner)
            .ToList();
        if (targets.Count == 0) {
            return;
        }

        Flash();
        foreach (Creature creature in targets) {
            await CreatureCmd.GainBlock(creature, Amount, ValueProp.Unpowered, null);
        }
    }
}
