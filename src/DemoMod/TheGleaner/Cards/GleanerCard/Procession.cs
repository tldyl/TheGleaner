using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Procession : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(12, ValueProp.Move),
		new IntVar("ReduceVal", 1)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this), HoverTipFactory.FromKeyword(CustomEnums.Score)];

	public Procession() : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		Vector2 windowSize = NRun.Instance.CombatRoom.Ui.GetViewport().GetVisibleRect().Size;
		GleanerVfxCmd.PlayVfx(new Vector2(windowSize.X * 0.65f, windowSize.Y * 0.5f), "res://TheGleaner/scenes/vfx/aoe_attack.tscn", 0.5f);
		await CreatureCmd.TriggerAnim(Owner.Creature, "AoEAttack", 0.5f);
		await using AttackContext context = await AttackCommand.CreateContextAsync(Owner.Creature.CombatState, this);
		IEnumerable<DamageResult> damageResults = await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, DynamicVars.Damage, Owner.Creature,
			this);
		context.AddHit(damageResults);
		int count = damageResults.Count(result => result.WasTargetKilled);
		if (count == damageResults.Count() - 1) {
			GleanerVfxCmd.PlayVfx(new Vector2(windowSize.X * 0.65f, windowSize.Y * 0.5f), "res://TheGleaner/scenes/vfx/aoe_attack.tscn");
			IEnumerable<DamageResult> _ = await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, DynamicVars.Damage, Owner.Creature,
				this);
		}
	}

	public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source) {
		if (oldPileType == CustomEnums.ScorePile && !Owner.Deck.Cards.Contains(this)) {
			EnergyCost.AddThisTurn(-DynamicVars["ReduceVal"].IntValue);
			AccessTools.Method(typeof(NPlayerHand), "OnCombatStateChanged", [typeof(CombatState)]).Invoke(NRun.Instance.CombatRoom.Ui.Hand, [Owner.Creature.CombatState]);
		}
	}
	
	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}
