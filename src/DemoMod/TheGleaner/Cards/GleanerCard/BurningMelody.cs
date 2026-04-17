using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class BurningMelody : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
		HoverTipFactory.Static(StaticHoverTip.Energy)
	];
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new EnergyVar(2),
		new EnergyVar("EnergyCard", 1)
	];

	public BurningMelody() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		CardPile pile = ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState);
		CardSelectorPrefs prefs = new CardSelectorPrefs(
			new LocString("cards", "DEMOMOD-SONOTOXIN.selectionScreenPrompt"),
			1
		);

		IEnumerable<CardModel> selected = await CardSelectCmd.FromSimpleGrid(
			choiceContext,
			pile.Cards,
			Owner,
			prefs
		);

		List<CardModel> selectedCards = selected.ToList();
		if (selectedCards.Count == 0) {
			return;
		}

		CardModel card = selectedCards.First();
		await CardCmd.Exhaust(choiceContext, card);

		await Hook.AfterCardChangedPiles(
			Owner.RunState,
			Owner.Creature.CombatState,
			card,
			CustomEnums.ScorePile,
			this
		);

		if (card.Type is CardType.Status or CardType.Curse) {
			await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
		} else {
			await PlayerCmd.GainEnergy(card.EnergyCost.GetResolved(), Owner);
		}
	}

	protected override void OnUpgrade() => DynamicVars.Energy.UpgradeValueBy(1);
}
