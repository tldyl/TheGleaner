using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Standoff : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars =>
		new List<DynamicVar> { new DamageVar(10, ValueProp.Move) };

	public Standoff() : base(0, CardType.Attack, CardRarity.Common, TargetType.AllEnemies, true, true) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		IEnumerable<Creature> allEnemies = CombatState.HittableEnemies.Where(enemy => enemy.Monster.IntendsToAttack);
		foreach (Creature creature in allEnemies) {
			GleanerVfxCmd.PlayOnCreature(creature, "res://TheGleaner/scenes/vfx/arrow_attack.tscn", 0.3f);
		}
		await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", 0.5f);
		foreach (Creature creature in allEnemies) {
			GleanerVfxCmd.PlayOnCreature(creature, "res://TheGleaner/scenes/vfx/arrow_hit_vfx.tscn");
		}
		await using AttackContext context = await AttackCommand.CreateContextAsync(Owner.Creature.CombatState, this);
		IEnumerable<DamageResult> damageResults = await CreatureCmd.Damage(choiceContext, allEnemies, DynamicVars.Damage, Owner.Creature, this);
		context.AddHit(damageResults);
	}

	protected override void OnUpgrade() {
		DynamicVars.Damage.UpgradeValueBy(3);
	}
}
