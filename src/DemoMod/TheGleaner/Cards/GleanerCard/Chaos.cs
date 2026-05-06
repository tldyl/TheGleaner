using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Chaos : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<WeakPower>(),
		HoverTipFactory.FromPower<VulnerablePower>(),
		HoverTipFactory.FromPower<StrengthPower>(),
		HoverTipFactory.FromPower<DexterityPower>(),
	];
	public override IEnumerable<CardKeyword> CanonicalKeywords => [
		CardKeyword.Exhaust
	];
	protected override bool HasEnergyCostX => true;

	public Chaos() : base(0, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await PowerCmd.Apply<WeakPower>(cardPlay.Target, ResolveEnergyXValue() + CurrentUpgradeLevel, Owner.Creature, this);
		await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, ResolveEnergyXValue() + CurrentUpgradeLevel, Owner.Creature, this);
		for (int _ = 0; _ < ResolveEnergyXValue() + CurrentUpgradeLevel; _++) {
			PowerModel powerModel = Owner.Creature.CombatState.RunState.Rng.CombatTargets.NextBool() ? ModelDb.Power<StrengthPower>() : ModelDb.Power<DexterityPower>();
			powerModel = powerModel.ToMutable();
			await PowerCmd.Apply(powerModel, Owner.Creature, 1, Owner.Creature, this);
		}
	}
}
