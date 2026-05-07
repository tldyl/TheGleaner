using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Encounters;

public sealed class GleanerCastleAndPawnsEncounter : EncounterModel
{
	public override RoomType RoomType => RoomType.Monster;

	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
	[
		ModelDb.Monster<GleanerCastle>(),
		ModelDb.Monster<GleanerPawn>()
	];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return
		[
			(ModelDb.Monster<GleanerPawn>().ToMutable(), null),
			(ModelDb.Monster<GleanerCastle>().ToMutable(), null),
			(ModelDb.Monster<GleanerPawn>().ToMutable(), null)
		];
	}

	public override float GetCameraScaling()
	{
		return 0.95f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 30f;
	}
}

public sealed class GleanerPawn : MonsterModel
{
	public const string AdvanceMoveId = "ADVANCE_MOVE";
	public const string DeterrenceMoveId = "DETERRENCE_MOVE";

	private const int InitialHp = 45;
	private const int AdvanceDamage = 10;
	private const int DeterrenceDamage = 5;
	private const int DeterrenceWeak = 1;
	private const int PromotionActions = 3;
	private const int PromotionStrength = 20;
	private const string TurretOperatorAttackSfx = "event:/sfx/enemy/enemy_attacks/turret_operator/turret_operator_attack";

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override string HurtSfx => "event:/sfx/enemy/enemy_attacks/turret_operator/turret_operator_hurt";

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

	protected override string AttackSfx => TurretOperatorAttackSfx;

	protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/turret_operator");

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<GleanerPromotionPower>(Creature, PromotionActions, Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState advance = new(AdvanceMoveId, AdvanceMove, new SingleAttackIntent(AdvanceDamage));
		MoveState deterrence = new(DeterrenceMoveId, DeterrenceMove, new SingleAttackIntent(DeterrenceDamage), new DebuffIntent());

		advance.FollowUpState = deterrence;
		deterrence.FollowUpState = advance;

		return new MonsterMoveStateMachine([advance, deterrence], advance);
	}

	private async Task AdvanceMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await DamageCmd.Attack(AdvanceDamage)
			.FromMonster(this)
			.WithAttackerAnim("Attack", 0.4f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await AdvancePromotion();
	}

	private async Task DeterrenceMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(DeterrenceDamage)
			.FromMonster(this)
			.WithAttackerAnim("Attack", 0.4f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);

		IReadOnlyList<Creature> playerTargets = targets.Where(static target => target.IsAlive && target.IsPlayer).ToList();
		await PowerCmd.Apply<WeakPower>(playerTargets, DeterrenceWeak, Creature, null);
		await AdvancePromotion();
	}

	private async Task AdvancePromotion()
	{
		GleanerPromotionPower? promotion = Creature.GetPower<GleanerPromotionPower>();
		if (promotion == null)
		{
			return;
		}

		promotion.TriggerFlash();
		if (promotion.Amount > 1)
		{
			await PowerCmd.ModifyAmount(promotion, -1m, Creature, null);
			return;
		}

		await PowerCmd.Remove(promotion);
		await PowerCmd.Apply<StrengthPower>(Creature, PromotionStrength, Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState idle = new("idle_loop", isLooping: true);
		AnimState crank = new("crank");
		AnimState attack = new("attack");
		AnimState hurt = new("hurt");
		AnimState die = new("die");
		crank.NextState = idle;
		attack.NextState = idle;
		hurt.NextState = idle;

		CreatureAnimator animator = new(idle, controller);
		animator.AddAnyState("Crank", crank);
		animator.AddAnyState("Attack", attack);
		animator.AddAnyState("Dead", die);
		animator.AddAnyState("Hit", hurt);
		return animator;
	}
}

public sealed class GleanerCastle : MonsterModel
{
	public const string IronCurtainBaptismMoveId = "IRON_CURTAIN_BAPTISM_MOVE";

	private const int InitialHp = 70;
	private const int IronCurtainDamage = 15;
	private const int IronCurtainBlock = 10;
	private const string LivingShieldAttackSfx = "event:/sfx/enemy/enemy_attacks/living_shield/living_shield_attack";

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override bool HasDeathSfx => false;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	protected override string AttackSfx => LivingShieldAttackSfx;

	protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/living_shield");

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<MinionPower>(Creature, 1m, Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState ironCurtainBaptism = new(
			IronCurtainBaptismMoveId,
			IronCurtainBaptismMove,
			new SingleAttackIntent(IronCurtainDamage),
			new DefendIntent());

		ironCurtainBaptism.FollowUpState = ironCurtainBaptism;
		return new MonsterMoveStateMachine([ironCurtainBaptism], ironCurtainBaptism);
	}

	private async Task IronCurtainBaptismMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await DamageCmd.Attack(IronCurtainDamage)
			.FromMonster(this)
			.WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);

		foreach (Creature creature in CombatState.Creatures.Where(creature => creature.IsAlive && creature != Creature))
		{
			await CreatureCmd.GainBlock(creature, IronCurtainBlock, ValueProp.Move, null);
		}
	}
}

public sealed class GleanerPromotionPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public void TriggerFlash()
	{
		Flash();
	}
}
