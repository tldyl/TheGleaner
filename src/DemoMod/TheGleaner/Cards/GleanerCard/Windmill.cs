using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Windmill : CustomCardModel {
	//public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new BlockVar(7, ValueProp.Move),
		new DamageVar(5, ValueProp.Move),
		new RepeatVar(4),
		new IntVar("Debuff", 1)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.Static(StaticHoverTip.Block),
		HoverTipFactory.FromPower<WeakPower>(),
		HoverTipFactory.FromPower<VulnerablePower>()
	];

	public Windmill() : base(2, CardType.Attack, CardRarity.Ancient, TargetType.AllEnemies) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", 0.5f);
		AttackContext context = await AttackCommand.CreateContextAsync(Owner.Creature.CombatState, this);
		for (int _ = 0; _ < DynamicVars.Repeat.IntValue; _++) {
			IEnumerable<DamageResult> damageResults =
				await CreatureCmd.Damage(choiceContext, Owner.Creature.CombatState.HittableEnemies, DynamicVars.Damage, Owner.Creature, this);
			context.AddHit(damageResults);

			int count = damageResults.Count(result => result.WasTargetKilled);

			if (count == damageResults.Count() - 1) {
				await PowerCmd.Apply<WeakPower>(
					Owner.Creature.CombatState.HittableEnemies,
					DynamicVars["Debuff"].BaseValue,
					Owner.Creature,
					this
				);
				await PowerCmd.Apply<VulnerablePower>(
					Owner.Creature.CombatState.HittableEnemies,
					DynamicVars["Debuff"].BaseValue,
					Owner.Creature,
					this
				);
			}
		}
	}

	protected override void OnUpgrade() {
		DynamicVars.Damage.UpgradeValueBy(2);
		DynamicVars.Block.UpgradeValueBy(3);
	}
}
