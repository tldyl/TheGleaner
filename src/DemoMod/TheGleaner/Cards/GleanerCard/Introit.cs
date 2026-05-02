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
public class Introit : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(10, ValueProp.Move),
		new IntVar("TakeAmount", 3),
		new CardsVar(1)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CustomEnums.Score)
	];

	public Introit() : base(2, CardType.Attack, CardRarity.Common, TargetType.AllEnemies) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await CreatureCmd.Damage(
			choiceContext,
			CombatState.HittableEnemies,
			DynamicVars.Damage,
			Owner.Creature,
			this
		);
		
		CardPile drawPile = PileType.Draw.GetPile(Owner);
		if (drawPile.Cards.Count == 0) {
			return;
		}

		CardSelectorPrefs prefs = new CardSelectorPrefs(
			new LocString("cards", "DEMOMOD-INTROIT.selectionScreenPrompt"),
			DynamicVars.Cards.IntValue
		);

		IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromSimpleGrid(
			choiceContext,
			PileType.Draw.GetPile(Owner).Cards
				.ToList().StableShuffle(Owner.RunState.Rng.CombatCardSelection).Take(DynamicVars["TakeAmount"].IntValue).ToList(),
			Owner,
			prefs
		);
		foreach (CardModel selectedCard in selectedCards) {
			await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, selectedCard);
			CardCmd.Preview(selectedCard);
		}
	}
	
	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}
