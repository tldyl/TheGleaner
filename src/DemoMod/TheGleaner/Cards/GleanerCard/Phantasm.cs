using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;
using BaseLib.Patches.Content;
using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Hooks;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Phantasm : CustomCardModel, IAfterTakeCardsFromScore {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	public override bool GainsBlock => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		EnergyHoverTip,
		HoverTipFactory.FromKeyword(CustomEnums.Score)
	];

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new BlockVar(6, ValueProp.Move),
		new IntVar("Times", 2),
		new EnergyVar(1)
	];

	public Phantasm() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true, true) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		int times = DynamicVars["Times"].IntValue;
		for (int i = 0; i < times; i++) {
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		}
	}

	public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side) {
		if (side != CombatSide.Player) {
			return;
		}
		
		CardPile scorePile = CustomPiles.GetCustomPile(Owner.PlayerCombatState, CustomEnums.ScorePile);

		if (scorePile != null && scorePile.Cards.Contains(this)) {
			EnergyCost.AddUntilPlayed(-1);
			return;
		}

		if (Owner.PlayerCombatState.Hand.Cards.Contains(this)) {
			CardCmd.Preview(this);
			await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
			NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(Owner.Creature);
			if (creatureNode != null) {
				Node2D spineNode = creatureNode.Visuals.GetNode<Node2D>("%GhostVisuals");
				MegaSprite sprite = new MegaSprite(spineNode);
				MegaTrackEntry track = sprite.GetAnimationState().SetAnimation("idle_loop");
				creatureNode.Visuals.GetNode<SubViewportContainer>("SubViewportContainer").Visible = true;
			}
		}
	}

	protected override void OnUpgrade() {
		  DynamicVars.Block.UpgradeValueBy(2);
	}

	public override async Task AfterCardChangedPiles(
		CardModel card,
		PileType oldPileType,
		AbstractModel? source) {
		if (card != this) {
			return;
		}
		if (card.Pile is ScorePile) {
			NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(Owner.Creature);
			if (creatureNode != null) {
				Node2D spineNode = creatureNode.Visuals.GetNode<Node2D>("%GhostVisuals");
				MegaSprite sprite = new MegaSprite(spineNode);
				MegaTrackEntry track = sprite.GetAnimationState().SetAnimation("idle_loop");
				creatureNode.Visuals.GetNode<SubViewportContainer>("SubViewportContainer").Visible = true;
			}
		}
	}
	
	public async Task AfterTakeCardsFromScore(CardModel card) {
		if (card != this) {
			return;
		}
		ScorePile scorePile = ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState);
		if (scorePile.Cards.Any(c => c is Phantasm)) { //如果乐谱中还有其他的罔象
			return;
		}
		NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(Owner.Creature);
		if (creatureNode != null) {
			creatureNode.Visuals.GetNode<SubViewportContainer>("SubViewportContainer").Visible = false;
		}
	}
}
