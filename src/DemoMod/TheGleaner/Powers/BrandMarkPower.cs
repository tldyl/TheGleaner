using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Powers;

public class BrandMarkPower : CustomPowerModel {
	public override string CustomPackedIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
	public override string CustomBigIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
	public override PowerType Type => PowerType.Debuff;
	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task BeforeDamageReceived(
		PlayerChoiceContext choiceContext,
		Creature target,
		Decimal amount,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource) {
		if (target == Owner && !props.HasFlag(ValueProp.Unpowered) && dealer != null) {
			Flash();
			await CreatureCmd.GainBlock(dealer, 2, ValueProp.Unpowered, null);
			await PowerCmd.Decrement(this);
		}
	}
}
