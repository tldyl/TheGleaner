using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;
using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.ValueProps;
using Godot;
using MegaCrit.Sts2.Core.Commands.Builders;
using DemoMod.TheGleaner.Utils;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class EarthedBell : CustomCardModel, IConcertoCard {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(0, ValueProp.Move),
		new BlockVar(4, ValueProp.Move)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CustomEnums.Concerto)
	];
	public override bool GainsBlock => true;

	public EarthedBell() : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		Vector2 windowSize = NRun.Instance.CombatRoom.Ui.GetViewport().GetVisibleRect().Size;
		GleanerVfxCmd.PlayVfx<Node2D>(new Vector2(windowSize.X * 0.65f, windowSize.Y * 0.5f), "res://TheGleaner/scenes/vfx/erathed_bell_vfx.tscn", 0.333f);
		await CreatureCmd.TriggerAnim(Owner.Creature, "AoEAttack", 0.5f);
		SoundManager.Instance.PlaySound(SoundKeys.GetSoundResourcePath("BELL_" + new Random().Next(1, 4) + "_ATTACK"), 1.0f);
		foreach (Creature target in Owner.Creature.CombatState.HittableEnemies) {
			GleanerVfxCmd.PlayOnCreature<Node2D>(target, "res://TheGleaner/scenes/vfx/prismatic_strike_hit_vfx.tscn");
		}
		AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.TargetingAllOpponents(Owner.Creature.CombatState)
			.WithNoAttackerAnim()
			.Execute(choiceContext);
	}
	
	public override Decimal ModifyDamageAdditive(
		Creature? target,
		Decimal amount,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource) {
		if (cardSource == this && !props.HasFlag(ValueProp.Unpowered)) {
			return Owner.Creature.Block;
		}
		return 0M;
	}

	protected override void OnUpgrade() {
		DynamicVars.Block.UpgradeValueBy(2);
	}

	public async Task OnConcerto(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		SoundManager.Instance.PlaySound(SoundKeys.GetSoundResourcePath("BELL_" + new Random().Next(1, 4) + "_CONCERTO"), 1.0f);
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
	}
}
