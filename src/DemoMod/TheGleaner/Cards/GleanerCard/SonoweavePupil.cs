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
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class SonoweavePupil : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new BlockVar(7, ValueProp.Move),
		new IntVar("CardAmount", 3),
		new IntVar("PlayAmount", 1)
	];
	public override bool GainsBlock => true;
	public SonoweavePupil() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
		
	}
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];    
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
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
