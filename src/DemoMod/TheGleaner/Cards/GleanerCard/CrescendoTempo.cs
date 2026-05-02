using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class CrescendoTempo : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(10, ValueProp.Move),
		new IntVar("Increase", 3),
		new IntVar("Sum", 0),
	];
	public override IEnumerable<CardKeyword> CanonicalKeywords => [
		CardKeyword.Ethereal
	];

	public CrescendoTempo() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);
	}

	public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side) {
		if (side != CombatSide.Player) {
			return;
		}
		DynamicVars["Sum"].BaseValue += DynamicVars["Increase"].BaseValue;
	}
	
	public override Decimal ModifyDamageAdditive(
		Creature? target,
		Decimal amount,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource) {
		if (cardSource == this && !props.HasFlag(ValueProp.Unpowered)) {
			return DynamicVars["Sum"].BaseValue;
		}
		return 0;
	}
	
	protected override void OnUpgrade() => DynamicVars["Increase"].UpgradeValueBy(1);
}
