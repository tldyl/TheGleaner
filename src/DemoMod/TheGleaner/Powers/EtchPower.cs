using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Powers;

public class EtchPower : CustomPowerModel {
	public override string CustomPackedIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
	public override string CustomBigIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
	public override PowerType Type => PowerType.Debuff;
	public override PowerStackType StackType => PowerStackType.Counter;
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<PoisonPower>()
	];

	public override async Task AfterDamageReceived(
		PlayerChoiceContext choiceContext,
		Creature target,
		DamageResult result,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource) {
		if (target == Owner && !props.HasFlag(ValueProp.Unpowered) && dealer != null && Owner.HasPower<PoisonPower>()) {
			Flash();
			PoisonPower poisonPower = Owner.GetPower<PoisonPower>();
			await poisonPower.AfterSideTurnStart(Owner.Side, Owner.CombatState);
			await PowerCmd.Decrement(this);
		}
	}
}
