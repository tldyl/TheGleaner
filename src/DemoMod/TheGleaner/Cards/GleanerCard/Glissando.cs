using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Glissando : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new IntVar("Amount", 1),
		new DamageVar(6, ValueProp.Move),
		new BlockVar(6, ValueProp.Move),
		new PowerVar<VulnerablePower>(1)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<WeakPower>(),
		HoverTipFactory.FromPower<VulnerablePower>()
	];

	public Glissando() : base(2, CardType.Attack, CardRarity.Basic, TargetType.AllEnemies) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		Glissando card = this;
		Decimal _ = await CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.Block, cardPlay);
		Vector2 windowSize = NRun.Instance.CombatRoom.Ui.GetViewport().GetVisibleRect().Size;
		GleanerVfxCmd.PlayVfx<Node2D>(new Vector2(windowSize.X * 0.65f, windowSize.Y * 0.5f), "res://TheGleaner/scenes/vfx/aoe_attack.tscn", 0.5f);
		await CreatureCmd.TriggerAnim(Owner.Creature, "AoEAttack", 0.5f);
		AttackCommand attackCommand = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.TargetingAllOpponents(Owner.Creature.CombatState)
			.WithNoAttackerAnim()
			.Execute(choiceContext);
		IEnumerable<DamageResult> damageResults = attackCommand.Results;

		int count = damageResults.Count(result => result.WasTargetKilled);

		if (count == damageResults.Count() - 1) {
			await PowerCmd.Apply<WeakPower>(
				CombatState.HittableEnemies,
				DynamicVars["Amount"].BaseValue,
				Owner.Creature,
				this
			);

			await PowerCmd.Apply<VulnerablePower>(
				CombatState.HittableEnemies,
				DynamicVars.Vulnerable.BaseValue,
				Owner.Creature,
				this
			);
		}
	}

	protected override void OnUpgrade() {
		DynamicVars.Damage.UpgradeValueBy(2);
		DynamicVars.Block.UpgradeValueBy(2);
	}
}
