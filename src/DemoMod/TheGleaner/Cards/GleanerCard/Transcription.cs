using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Transcription : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Amount", 1)];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Score)];

	public Transcription() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self) {
	}
	
	public override IEnumerable<CardKeyword> CanonicalKeywords => [
		CardKeyword.Exhaust
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		CardPile scorePile = CustomPiles.GetCustomPile(Owner.PlayerCombatState, CustomEnums.ScorePile);
		if (scorePile == null || scorePile.Cards.Count == 0) {
			return;
		}

		CardSelectorPrefs prefs = new CardSelectorPrefs(new LocString("cards", "DEMOMOD-TRANSCRIPTION.selectionScreenPrompt"), 1);
		CardModel selectedCard = (await CardSelectCmd.FromSimpleGrid(choiceContext, scorePile.Cards, Owner, prefs)).FirstOrDefault();
		if (selectedCard == null) {
			return;
		}

		for (int i = 0; i < DynamicVars["Amount"].IntValue; i++) {
			CardModel copy = selectedCard.CreateClone();
			await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, true);
		}
	}

		protected override void OnUpgrade() {
		RemoveKeyword(CardKeyword.Exhaust);
	}
}
