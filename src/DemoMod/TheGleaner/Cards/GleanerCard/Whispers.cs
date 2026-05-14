using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Whispers : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new CardsVar(1),
		new IntVar("Draw", 2)
		
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CustomEnums.Dissonance),
		HoverTipFactory.FromCard<DirgeOfFarewell>(),
		HoverTipFactory.FromCard<ShriekOfDread>(),
		HoverTipFactory.FromCard<HowlOfWrath>()
	];

	public Whispers() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
		
	}

	public override IEnumerable<CardKeyword> CanonicalKeywords => [
		CardKeyword.Ethereal
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		CardPile drawPile = PileType.Draw.GetPile(Owner);
		if (drawPile.Cards.Count == 0) {
			return;
		}

		CardSelectorPrefs prefs = new CardSelectorPrefs(
			new LocString("cards", "DEMOMOD-WHISPERS.selectionScreenPrompt"),
			DynamicVars.Cards.IntValue
		);

		IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromSimpleGrid(
			choiceContext,
			drawPile.Cards,
			Owner,
			prefs
		);
		SoundManager.Instance.PlaySound(SoundKeys.HEART_BEAT);
		foreach (CardModel selectedCard in selectedCards) {
			CardModel card = Owner.Creature.CombatState.CreateCard(RandomDissonanceCard.getRandomDissonanceCards(1, Owner.RunState.Rng.CombatCardGeneration)[0], Owner);
			await CardCmd.Transform(selectedCard, card);
		}
		await CardPileCmd.Draw(choiceContext, DynamicVars["Draw"].BaseValue, Owner, false);
	}

		protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}
