using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Stop : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<SlowPower>()
	];
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	public Stop() : base(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		if (CurrentUpgradeLevel > 0) {
			await PowerCmd.Apply<SlowPower>(Owner.Creature.CombatState.HittableEnemies, 1, Owner.Creature, this);
		} else {
			await PowerCmd.Apply<SlowPower>(cardPlay.Target, 1, Owner.Creature, this);
		}
	}

	protected override void OnUpgrade() {
		AccessTools.Field(typeof(CardModel), "<TargetType>k__BackingField").SetValue(this, TargetType.AllEnemies);
	}
}
