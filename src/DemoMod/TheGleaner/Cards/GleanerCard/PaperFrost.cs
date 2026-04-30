using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class PaperFrost : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	public override IEnumerable<CardKeyword> CanonicalKeywords => [
		CardKeyword.Exhaust
	];

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new IntVar("Amount", 4)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<StrengthPower>(),
		HoverTipFactory.FromKeyword(CustomEnums.Score)
	];

	public override async Task BeforeCombatStart() {
		if (!IsInCombat || CombatState == null || Owner.Deck.Cards.Contains(this)) {
			return;
		}
		
		await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
	}
	
	public PaperFrost() : base(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		if (CurrentUpgradeLevel > 0) {
			CardModel cpy = CreateClone();
			cpy.DowngradeInternal();
			await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, cpy);
			CardCmd.Preview(cpy);
		}
		await PowerCmd.Apply<DemoTempLoseStrengthPower>(
			Owner.Creature.CombatState.HittableEnemies,
			-DynamicVars["Amount"].BaseValue,
			Owner.Creature,
			this
		);
	}
}
