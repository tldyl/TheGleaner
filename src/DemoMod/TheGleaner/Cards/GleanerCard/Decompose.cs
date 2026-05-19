using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Decompose : CustomCardModel
{
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new PowerVar<PoisonPower>(6),
		new IntVar("PoisonThreshold", 10),
		new PowerVar<EtchPower>(1),
		new IntVar("EtchPowerPreview", 0)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<EtchPower>(),
		HoverTipFactory.FromPower<PoisonPower>()
	];

	public Decompose() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy) {
	}
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await PowerCmd.Apply<PoisonPower>(cardPlay.Target, DynamicVars["PoisonPower"].BaseValue, Owner.Creature, this);
		int amount = 0;
		if (cardPlay.Target.HasPower<PoisonPower>()) {
			PowerModel power = cardPlay.Target.GetPower<PoisonPower>();
			amount += power.Amount / DynamicVars["PoisonThreshold"].IntValue;
			amount *= DynamicVars["EtchPower"].IntValue;
		}
		if (amount > 0) {
			await PowerCmd.Apply<EtchPower>(cardPlay.Target, amount, Owner.Creature, this);
		}
	}

	public override Decimal ModifyDamageAdditive(
		Creature? target,
		Decimal amount,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource) {
		if (target != null) {
			int powerAmount = 0;
			if (target.HasPower<PoisonPower>()) {
				PowerModel power = target.GetPower<PoisonPower>();
				powerAmount += power.Amount / DynamicVars["PoisonThreshold"].IntValue;
				powerAmount *= DynamicVars["EtchPower"].IntValue;
			}
			DynamicVars["EtchPowerPreview"].BaseValue = powerAmount;
		}
		return 0;
	}
	
	protected override void OnUpgrade() {
		DynamicVars["PoisonPower"].UpgradeValueBy(3);
	}

}
