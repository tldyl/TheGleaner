using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class WeavememoryBlade : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(4, ValueProp.Move),
		new CardsVar(1)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CustomEnums.Resonance),
		HoverTipFactory.FromKeyword(CustomEnums.Score)
	];

	public WeavememoryBlade() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
		
	}

public override IEnumerable<CardKeyword> CanonicalKeywords => [
		CardKeyword.Exhaust
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);
		if (IsUpgraded) {
			CardSelectorPrefs prefs = new CardSelectorPrefs(
				new LocString("cards", "DEMOMOD-WEAVEMEMORY_BLADE.selectionScreenPrompt"),
				DynamicVars.Cards.IntValue
			);

			IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromSimpleGrid(
				choiceContext,
				PileType.Discard.GetPile(Owner).Cards.Where(c => c.Keywords.Contains(CustomEnums.Resonance)).ToList(),
				Owner,
				prefs
			);
			foreach (CardModel selectedCard in selectedCards) {
				await CardPileCmd.Add(selectedCard, PileType.Hand, CardPilePosition.Bottom, null, false);
			}
		} else {
			IEnumerable<CardModel> selectedCards = PileType.Discard.GetPile(Owner).Cards.Where(c => c.Keywords.Contains(CustomEnums.Resonance))
				.ToList().StableShuffle(Owner.RunState.Rng.CombatCardSelection).Take(DynamicVars.Cards.IntValue);
			foreach (CardModel selectedCard in selectedCards) {
				await CardPileCmd.Add(selectedCard, PileType.Hand, CardPilePosition.Bottom, null, false);
			}
		}
	}
	
	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}
