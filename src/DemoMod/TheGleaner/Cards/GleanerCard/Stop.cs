using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Nodes.Vfx;
using DemoMod.TheGleaner.Pools;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Stop : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<SlowPower>()
	];
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	public Stop() : base(1, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await PowerCmd.Apply<SlowPower>(Owner.Creature.CombatState.HittableEnemies, 1, Owner.Creature, this);
		NGrayGradientVfxPostProcessor.Instance.ToggleBlackAndWhite(true);
		NCreature nCreature = NCombatRoom.Instance.GetCreatureNode(Owner.Creature);
		if (nCreature != null) {
			Vector2 screenSize = NGame.Instance.GetViewportRect().Size;
			Vector2 uv = new Vector2(nCreature.GlobalPosition.X / screenSize.X, nCreature.GlobalPosition.Y / screenSize.Y);
			NGrayGradientVfxPostProcessor.Instance.TriggerExpand(uv, 0.5f);
		}
	}
	
	protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
