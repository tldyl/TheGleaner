using BaseLib.Abstracts;
using Godot;
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
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Encounters;

public sealed class GleanerIvoryAndEbonyEncounter : EncounterModel
{
	public override RoomType RoomType => RoomType.Elite;

	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
	[
		ModelDb.Monster<GleanerIvoryBishop>(),
		ModelDb.Monster<GleanerEbonyBishop>()
	];

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return
		[
			(ModelDb.Monster<GleanerIvoryBishop>().ToMutable(), null),
			(ModelDb.Monster<GleanerEbonyBishop>().ToMutable(), null)
		];
	}

	public override float GetCameraScaling()
	{
		return 0.95f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 25f;
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

	protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/twig_slime_m");

	protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/twig_slime_m/twig_slime_m_attack";

	protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/twig_slime_m/twig_slime_m_cast";

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
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_slime_impact")
			.Execute(null);
	}

	private async Task ConvictionMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ConvictionDamage)
			.FromMonster(this)
			.WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, AttackSfx)
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

	protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/leaf_slime_m");

	protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/leaf_slime_m/leaf_slime_m_attack";

	protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/leaf_slime_m/leaf_slime_m_cast";

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
			.WithAttackerFx(null, AttackSfx)
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
			.WithAttackerFx(null, AttackSfx)
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
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_slime_impact")
			.Execute(null);
	}
}

public abstract class GleanerBishop : MonsterModel
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
		SfxCmd.Play(CastSfx);
		await CreatureCmd.TriggerAnim(Creature, "Cast", 0.75f);
	}

	private static bool IsLivingBishop(Creature creature)
	{
		return creature.IsAlive && creature.Monster is GleanerIvoryBishop or GleanerEbonyBishop;
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
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

public sealed class GleanerWhiteSquareDomainPower : CustomPowerModel
{
	private const string PowerIconPath = "res://TheGleaner/images/powers/demomod-the_lake_mirror_power.png";

	public override string CustomPackedIconPath => PowerIconPath;

	public override string CustomBigIconPath => PowerIconPath;

	public override LocString Title => new("powers", "gleanerWhiteSquareDomainPower.title");

	public override LocString Description => new("powers", "gleanerWhiteSquareDomainPower.description");

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override string SmartDescriptionLocKey => "gleanerWhiteSquareDomainPower.smartDescription";

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<StrengthPower>()
	];

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner.Creature?.IsPlayer != true || cardPlay.Card.Type != CardType.Skill)
		{
			return;
		}

		Flash();
		await PowerCmd.Apply<StrengthPower>(Owner, Amount, Owner, null);
		await PowerCmd.Apply<GleanerStrengthDecayPower>(Owner, Amount, Owner, null);
	}
}

public sealed class GleanerBlackSquareDomainPower : CustomPowerModel
{
	private const string PowerIconPath = "res://TheGleaner/images/powers/demomod-one_winged_violin_power.png";

	public override string CustomPackedIconPath => PowerIconPath;

	public override string CustomBigIconPath => PowerIconPath;

	public override LocString Title => new("powers", "gleanerBlackSquareDomainPower.title");

	public override LocString Description => new("powers", "gleanerBlackSquareDomainPower.description");

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override string SmartDescriptionLocKey => "gleanerBlackSquareDomainPower.smartDescription";

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<StrengthPower>()
	];

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner.Creature?.IsPlayer != true || cardPlay.Card.Type != CardType.Attack)
		{
			return;
		}

		Flash();
		await PowerCmd.Apply<StrengthPower>(Owner, Amount, Owner, null);
		await PowerCmd.Apply<GleanerStrengthDecayPower>(Owner, Amount, Owner, null);
	}
}

public sealed class GleanerFanaticPower : CustomPowerModel
{
	private const string PowerIconPath = "res://TheGleaner/images/powers/demomod-rendezvous_with_doom_power.png";

	public override string CustomPackedIconPath => PowerIconPath;

	public override string CustomBigIconPath => PowerIconPath;

	public override LocString Title => new("powers", "gleanerFanaticPower.title");

	public override LocString Description => new("powers", "gleanerFanaticPower.description");

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override string SmartDescriptionLocKey => "gleanerFanaticPower.smartDescription";

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<GleanerStrengthDecayPower>()
	];

	public override async Task AfterDamageReceived(
		PlayerChoiceContext choiceContext,
		Creature target,
		DamageResult result,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource)
	{
		_ = choiceContext;
		_ = props;
		_ = dealer;
		if (target != Owner || result.UnblockedDamage <= 0 || cardSource?.Type != CardType.Attack)
		{
			return;
		}

		GleanerStrengthDecayPower? decay = Owner.GetPower<GleanerStrengthDecayPower>();
		if (decay == null)
		{
			return;
		}

		Flash();
		await PowerCmd.Remove(decay);
	}
}

public sealed class GleanerStrengthDecayPower : CustomPowerModel
{
	private const string PowerIconPath = "res://TheGleaner/images/powers/demomod-demo_temp_strength_power.png";

	public override string CustomPackedIconPath => PowerIconPath;

	public override string CustomBigIconPath => PowerIconPath;

	public override LocString Title => new("powers", "gleanerStrengthDecayPower.title");

	public override LocString Description => new("powers", "gleanerStrengthDecayPower.description");

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override string SmartDescriptionLocKey => "gleanerStrengthDecayPower.smartDescription";

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<StrengthPower>()
	];

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		_ = choiceContext;
		if (side != Owner.Side)
		{
			return;
		}

		Flash();
		int amount = Amount;
		await PowerCmd.Remove(this);
		await PowerCmd.Apply<StrengthPower>(Owner, -amount, Owner, null);
	}
}
