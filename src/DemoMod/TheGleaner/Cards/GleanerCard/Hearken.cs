using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Hearken : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DynamicVar("StrengthLoss", 7),
		new DynamicVar("StrengthLoss2", 3)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

	public Hearken() : base(2, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await PowerCmd.Apply<DampingPower>(cardPlay.Target, DynamicVars["StrengthLoss"].BaseValue, Owner.Creature, this);
		var otherEnemies = Owner.Creature.CombatState.GetTeammatesOf(cardPlay.Target)
	.Except([cardPlay.Target])
	.Where(e => e.IsHittable)
	.ToList();
	if (otherEnemies.Count > 0) {
	await PowerCmd.Apply<DampingPower>(otherEnemies, DynamicVars["StrengthLoss2"].BaseValue, Owner.Creature, this);
}
	}

	protected override void OnUpgrade() {
		DynamicVars["StrengthLoss"].UpgradeValueBy(1);
		DynamicVars["StrengthLoss2"].UpgradeValueBy(1);
	}
}
