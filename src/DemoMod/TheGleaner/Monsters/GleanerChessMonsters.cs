using BaseLib.Abstracts;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Monsters;

public sealed class GleanerPawn : CustomMonsterModel
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

	public override string CustomAttackSfx => TurretOperatorAttackSfx;

	public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/turret_operator");

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
			.WithAttackerFx(null, CustomAttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await AdvancePromotion();
	}

	private async Task DeterrenceMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(DeterrenceDamage)
			.FromMonster(this)
			.WithAttackerAnim("Attack", 0.4f)
			.WithAttackerFx(null, CustomAttackSfx)
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

	public override CreatureAnimator SetupCustomAnimationStates(MegaSprite controller)
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

public sealed class GleanerCastle : CustomMonsterModel
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

	public override string CustomAttackSfx => LivingShieldAttackSfx;

	public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/living_shield");

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
			.WithAttackerFx(null, CustomAttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);

		foreach (Creature creature in CombatState.Creatures.Where(creature => creature.IsAlive && creature != Creature))
		{
			await CreatureCmd.GainBlock(creature, IronCurtainBlock, ValueProp.Move, null);
		}
	}
}

public sealed class GleanerIvoryBishop : GleanerBishop
{
	public const string LightCrossMoveId = "LIGHT_CROSS_MOVE";
	public const string ConvictionMoveId = "CONVICTION_MOVE";
	public const string SanctuaryMoveId = "SANCTUARY_MOVE";

	private const int LightCrossDamage = 9;
	private const int LightCrossHits = 2;
	private const int ConvictionDamage = 7;
	private const int ConvictionVulnerable = 1;

	public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/twig_slime_m");

	public override string CustomAttackSfx => "event:/sfx/enemy/enemy_attacks/twig_slime_m/twig_slime_m_attack";

	public override string CustomCastSfx => "event:/sfx/enemy/enemy_attacks/twig_slime_m/twig_slime_m_cast";

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<GleanerWhiteSquareDomainPower>(Creature, DomainAmount, Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState lightCross = new(
			LightCrossMoveId,
			LightCrossMove,
			new MultiAttackIntent(LightCrossDamage, LightCrossHits));
		MoveState conviction = new(
			ConvictionMoveId,
			ConvictionMove,
			new SingleAttackIntent(ConvictionDamage),
			new DebuffIntent());
		MoveState sanctuary = new(
			SanctuaryMoveId,
			SanctuaryMove,
			new BuffIntent(),
			new DefendIntent());

		lightCross.FollowUpState = conviction;
		conviction.FollowUpState = sanctuary;
		sanctuary.FollowUpState = lightCross;

		return new MonsterMoveStateMachine([lightCross, conviction, sanctuary], lightCross);
	}

	private async Task LightCrossMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await DamageCmd.Attack(LightCrossDamage)
			.FromMonster(this)
			.WithHitCount(LightCrossHits)
			.WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, CustomAttackSfx)
			.WithHitFx("vfx/vfx_slime_impact")
			.Execute(null);
	}

	private async Task ConvictionMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ConvictionDamage)
			.FromMonster(this)
			.WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, CustomAttackSfx)
			.WithHitFx("vfx/vfx_slime_impact")
			.Execute(null);

		await PowerCmd.Apply<VulnerablePower>(LivingPlayerTargets(targets), ConvictionVulnerable, Creature, null);
	}

	private async Task SanctuaryMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await Cast();
		await ApplyPlatingToBishops(SanctuaryPlating);
	}
}

public sealed class GleanerEbonyBishop : GleanerBishop
{
	public const string PinDownMoveId = "PIN_DOWN_MOVE";
	public const string ProfanationMoveId = "PROFANATION_MOVE";
	public const string DarkCrossMoveId = "DARK_CROSS_MOVE";

	private const int PinDownDamage = 7;
	private const int PinDownDazed = 2;
	private const int ProfanationDamage = 13;
	private const int ProfanationWeak = 1;
	private const int DarkCrossDamage = 9;
	private const int DarkCrossHits = 2;

	public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/leaf_slime_m");

	public override string CustomAttackSfx => "event:/sfx/enemy/enemy_attacks/leaf_slime_m/leaf_slime_m_attack";

	public override string CustomCastSfx => "event:/sfx/enemy/enemy_attacks/leaf_slime_m/leaf_slime_m_cast";

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<GleanerBlackSquareDomainPower>(Creature, DomainAmount, Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState pinDown = new(
			PinDownMoveId,
			PinDownMove,
			new SingleAttackIntent(PinDownDamage),
			new StatusIntent(PinDownDazed));
		MoveState profanation = new(
			ProfanationMoveId,
			ProfanationMove,
			new SingleAttackIntent(ProfanationDamage),
			new DebuffIntent());
		MoveState darkCross = new(
			DarkCrossMoveId,
			DarkCrossMove,
			new MultiAttackIntent(DarkCrossDamage, DarkCrossHits));

		pinDown.FollowUpState = profanation;
		profanation.FollowUpState = darkCross;
		darkCross.FollowUpState = pinDown;

		return new MonsterMoveStateMachine([pinDown, profanation, darkCross], pinDown);
	}

	private async Task PinDownMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(PinDownDamage)
			.FromMonster(this)
			.WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, CustomAttackSfx)
			.WithHitFx("vfx/vfx_slime_impact")
			.Execute(null);

		await CardPileCmd.AddToCombatAndPreview<Dazed>(
			LivingPlayerTargets(targets),
			PileType.Draw,
			PinDownDazed,
			addedByPlayer: false,
			CardPilePosition.Random);
	}

	private async Task ProfanationMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ProfanationDamage)
			.FromMonster(this)
			.WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, CustomAttackSfx)
			.WithHitFx("vfx/vfx_slime_impact")
			.Execute(null);

		await PowerCmd.Apply<WeakPower>(LivingPlayerTargets(targets), ProfanationWeak, Creature, null);
	}

	private async Task DarkCrossMove(IReadOnlyList<Creature> targets)
	{
		_ = targets;
		await DamageCmd.Attack(DarkCrossDamage)
			.FromMonster(this)
			.WithHitCount(DarkCrossHits)
			.WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, CustomAttackSfx)
			.WithHitFx("vfx/vfx_slime_impact")
			.Execute(null);
	}
}

public abstract class GleanerBishop : CustomMonsterModel
{
	protected const int DomainAmount = 1;
	protected const int SanctuaryPlating = 4;

	private const int InitialHp = 180;
	private const int InitialPlating = 10;

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Slime;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<PlatingPower>(Creature, InitialPlating, Creature, null);
		await PowerCmd.Apply<GleanerFanaticPower>(Creature, 1m, Creature, null);
	}

	protected static IReadOnlyList<Creature> LivingPlayerTargets(IReadOnlyList<Creature> targets)
	{
		return targets.Where(static target => target.IsAlive && target.IsPlayer).ToList();
	}

	protected async Task ApplyPlatingToBishops(int amount)
	{
		foreach (Creature creature in CombatState.Creatures.Where(IsLivingBishop))
		{
			await PowerCmd.Apply<PlatingPower>(creature, amount, Creature, null);
		}
	}

	protected async Task Cast()
	{
		SfxCmd.Play(CustomCastSfx);
		await CreatureCmd.TriggerAnim(Creature, "Cast", 0.75f);
	}

	private static bool IsLivingBishop(Creature creature)
	{
		return creature.IsAlive && creature.Monster is GleanerIvoryBishop or GleanerEbonyBishop;
	}

	public override CreatureAnimator SetupCustomAnimationStates(MegaSprite controller)
	{
		AnimState idle = new("idle_loop", isLooping: true);
		AnimState attack = new("attack");
		AnimState cast = new("cast");
		AnimState hurt = new("hurt");
		AnimState die = new("die");
		attack.NextState = idle;
		cast.NextState = idle;
		hurt.NextState = idle;

		CreatureAnimator animator = new(idle, controller);
		animator.AddAnyState("Attack", attack);
		animator.AddAnyState("Cast", cast);
		animator.AddAnyState("Dead", die);
		animator.AddAnyState("Hit", hurt);
		return animator;
	}
}
