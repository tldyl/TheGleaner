using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class MimeticAmalgam : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new IntVar("CardAmount", 10),
		new IntVar("PlayAmount", 2)
	];
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	public MimeticAmalgam() : base(3, CardType.Skill, CardRarity.Rare, TargetType.Self) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		List<CardPoolModel> list1 = Owner.UnlockState.CharacterCardPools.ToList();
		if (list1.Count > 1) {
			list1.Remove(Owner.Character.CardPool);
		}
		IEnumerable<CardModel> cards = from c in list1.SelectMany(c => c.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint))
			where c.Type == CardType.Power
			select c;
		List<CardModel> list2 = CardFactory.GetDistinctForCombat(Owner, cards, DynamicVars["CardAmount"].IntValue, Owner.RunState.Rng.CombatCardGeneration).ToList();
		if (IsUpgraded) {
			foreach (CardModel card2 in list2) {
				CardCmd.Upgrade(card2);
			}
		}
		CardSelectorPrefs prefs = new CardSelectorPrefs(
			new LocString("cards", "DEMOMOD-MIMETIC_AMALGAM.selectionScreenPrompt"),
			DynamicVars["PlayAmount"].IntValue
		);
		IEnumerable<CardModel> selected = await CardSelectCmd.FromSimpleGrid(
			choiceContext,
			list2,
			Owner,
			prefs
		);
		List<CardModel> toPlay = [];
		toPlay.AddRange(selected);
		foreach (CardModel c in toPlay) {
			await CardCmd.AutoPlay(choiceContext, c, null);
		}
	}
}
