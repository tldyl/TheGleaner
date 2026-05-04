using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Utils;
using Godot;
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
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Introit : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(12, ValueProp.Move),
		new IntVar("TakeAmount", 3),
		new CardsVar(1)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CustomEnums.Score)
	];

	public Introit() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		Vector2 windowSize = NRun.Instance.CombatRoom.Ui.GetViewport().GetVisibleRect().Size;
		GleanerVfxCmd.PlayVfx<Node2D>(new Vector2(windowSize.X * 0.65f, windowSize.Y * 0.5f), "res://TheGleaner/scenes/vfx/aoe_attack.tscn", 0.5f);
		await CreatureCmd.TriggerAnim(Owner.Creature, "AoEAttack", 0.5f);
		AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.TargetingAllOpponents(Owner.Creature.CombatState)
			.WithNoAttackerAnim()
			.Execute(choiceContext);

		CardSelectorPrefs prefs = new CardSelectorPrefs(
			new LocString("cards", "DEMOMOD-INTROIT.selectionScreenPromptDraw"),
			0,
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
		
		prefs = new CardSelectorPrefs(
			new LocString("cards", "DEMOMOD-INTROIT.selectionScreenPromptDiscard"),
			0,
			DynamicVars.Cards.IntValue
		);
		selectedCards = await CardSelectCmd.FromSimpleGrid(
			choiceContext,
			PileType.Discard.GetPile(Owner).Cards
				.ToList().StableShuffle(Owner.RunState.Rng.CombatCardSelection).Take(DynamicVars["TakeAmount"].IntValue).ToList(),
			Owner,
			prefs
		);
		foreach (CardModel selectedCard in selectedCards) {
			await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, selectedCard);
			CardCmd.Preview(selectedCard);
		}
	}

	protected override void OnUpgrade() {
		DynamicVars.Damage.UpgradeValueBy(4);
		DynamicVars["TakeAmount"].UpgradeValueBy(1);
	} 
}
