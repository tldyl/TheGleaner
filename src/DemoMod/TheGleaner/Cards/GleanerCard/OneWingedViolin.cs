using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Nodes.Vfx;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using DemoMod.TheGleaner.Utils;
using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class OneWingedViolin : CustomCardModel {
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(6, ValueProp.Move)
	];
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	
	public OneWingedViolin() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
	}
	
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CustomEnums.Resonance];
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		if (!cardPlay.IsAutoPlay) {
			GleanerVfxCmd.PlayOnCreature<Node2D>(cardPlay.Target, "res://TheGleaner/scenes/vfx/arrow_attack.tscn", 0.3f);
			await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", 0.5f);
			GleanerVfxCmd.PlayOnCreature<Node2D>(cardPlay.Target, "res://TheGleaner/scenes/vfx/arrow_hit_vfx.tscn");
		} else {
			NPrismaticRainfallStrikeVfx vfx = NPrismaticRainfallStrikeVfx.Create(cardPlay.Target);
			GleanerVfxCmd.PlayVfx(new Vector2(), vfx);
			GleanerVfxCmd.PlayOnCreature<Node2D>(cardPlay.Target, "res://TheGleaner/scenes/vfx/prismatic_strike_hit_vfx.tscn", 0.3f);
			await Cmd.Wait(0.3f);
		}
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.WithNoAttackerAnim()
			.Execute(choiceContext);
	}
	
	protected override void OnUpgrade() {
		DynamicVars.Damage.UpgradeValueBy(3);
	}
}
