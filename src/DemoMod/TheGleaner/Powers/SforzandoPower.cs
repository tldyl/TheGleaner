using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Powers;

public class SforzandoPower : CustomPowerModel {
	public override string CustomPackedIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
	public override string CustomBigIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
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
	
}
