using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using DemoMod.TheGleaner.Utils;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class SnapPizzicato : CustomCardModel
{
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(8, ValueProp.Move),
		new CardsVar(2),
		new PowerVar<VulnerablePower>(1)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<VulnerablePower>(), HoverTipFactory.FromKeyword(CustomEnums.Score)
	];

	public SnapPizzicato() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		//TODO 效果改动
		if (!cardPlay.IsAutoPlay) {
			GleanerVfxCmd.PlayOnCreature<Node2D>(cardPlay.Target, "res://TheGleaner/scenes/vfx/arrow_attack.tscn", 0.3f);
			await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", 0.5f);
			GleanerVfxCmd.PlayOnCreature<Node2D>(cardPlay.Target, "res://TheGleaner/scenes/vfx/arrow_hit_vfx.tscn");
		}
		AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.WithNoAttackerAnim()
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);

		CardPile pile = ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState);
		CardSelectorPrefs prefs = new CardSelectorPrefs(
			new LocString("cards", "DEMOMOD-SNAP_PIZZICATO.selectionScreenPrompt"),
			0,
			DynamicVars.Cards.IntValue
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

		await CardCmd.Discard(choiceContext, selectedCards);

		foreach (CardModel card in selectedCards) {
			if (Owner.Creature.HasPower<StaffBurnoutPower>()) {
				await Owner.Creature.GetPower<StaffBurnoutPower>().AfterCardChangedPiles(card, CustomEnums.ScorePile, null);
			}
		}

		await PowerCmd.Apply<VulnerablePower>(
			cardPlay.Target,
			DynamicVars["VulnerablePower"].BaseValue * selectedCards.Count,
			Owner.Creature,
			this
		);
		GleanerVfxCmd.CheckScoreIsEmpty(Owner.PlayerCombatState);
	}

	protected override void OnUpgrade() {
		DynamicVars.Damage.UpgradeValueBy(2);
		DynamicVars.Cards.UpgradeValueBy(1);
	} 
}
