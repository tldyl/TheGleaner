using BaseLib.Abstracts;
using DemoMod.TheGleaner.Powers;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Monsters;

public class SleepyScarecrow : CustomMonsterModel {
    public const string SleepMoveId = "SLEEP_MOVE";
    public const string CurseMoveId = "CURSE_MOVE";
    public const string SpinningMoveId = "SPINNING_MOVE";
    public const string NightmareMoveId = "NIGHTMARE_MOVE";

    private const int InitialHp = 80;
    private const int SleepTurns = 3;
    private const int InitialPlating = 24;
    private const int CurseDamage = 10;
    private const int CurseWeak = 1;
    private const int SpinningDamage = 2;
    private const int SpinningHits = 10;
    private const int NightmareStrength = 4;
    private const string AwakenSfx = "event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_awaken";
    private const string SlamSfx = "event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_slam";
    private const string CastMoveSfx = "event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_cast";
    private const string StabSfx = "event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_attack_stab";

    private bool _isAwake;
    private bool _isShellAwake;
    private NSleepingVfx? _sleepingVfx;
    private MoveState? _curseState;

    public override int MinInitialHp => InitialHp;

    public override int MaxInitialHp => InitialHp;

    public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/lagavulin_matriarch");

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.ArmorBig;

    public bool IsAwake {
        get => _isAwake;
        set {
            AssertMutable();
            _isAwake = value;
        }
    }

    public bool IsShellAwake {
        get => _isShellAwake;
        private set {
            AssertMutable();
            _isShellAwake = value;
        }
    }

    private NSleepingVfx? SleepingVfx {
        get => _sleepingVfx;
        set {
            AssertMutable();
            _sleepingVfx = value;
        }
    }

    public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton) {
        spine.GetAnimationState().SetAnimation("_tracks/eyes_closed_loop", loop: true, 1);
    }

    public override async Task AfterAddedToRoom() {
        await base.AfterAddedToRoom();
        await Sleep();
    }

    public async Task WakeUp(bool startleCrows) {
        if (!IsAwake) {
            SfxCmd.Play(AwakenSfx);
            await CreatureCmd.TriggerAnim(Creature, "Wake", 0.6f);
            StopSleepingVfx();
            IsAwake = true;
        }

        if (Creature.GetPower<PlatingPower>() is { } plating) {
            await PowerCmd.Remove(plating);
        }

        if (_curseState != null) {
            SetMoveImmediate(_curseState, true);
        }

        if (!startleCrows) {
            return;
        }

        foreach (Creature creature in CombatState.Creatures.Where(static creature => creature.IsAlive && creature.Monster is Crow)) {
            await ((Crow)creature.Monster).StartledByScarecrow();
        }
    }

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource) {
        _ = choiceContext;
        _ = props;
        _ = dealer;
        _ = cardSource;
        if (target != Creature || result.UnblockedDamage <= 0) {
            return Task.CompletedTask;
        }

        StopSleepingVfx();
        if (Creature.CurrentHp <= Creature.MaxHp / 2 && !IsShellAwake) {
            NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(Creature);
            creatureNode?.SpineAnimation.SetAnimation("_tracks/eyes_open", loop: false, 1);
            creatureNode?.SpineAnimation.AddAnimation("_tracks/eyes_open_loop", 0f, loop: true, 1);
            IsShellAwake = true;
        }

        return Task.CompletedTask;
    }

    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength) {
        _ = choiceContext;
        _ = wasRemovalPrevented;
        _ = deathAnimLength;
        if (creature != Creature) {
            return Task.CompletedTask;
        }

        NCombatRoom.Instance?.GetCreatureNode(Creature)?.SpineAnimation.SetAnimation("_tracks/eyes_dead", loop: false, 1);
        StopSleepingVfx();
        return Task.CompletedTask;
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        MoveState sleep = new(SleepMoveId, SleepMove, new SleepIntent());
        MoveState curse = new(CurseMoveId, CurseMove, new SingleAttackIntent(CurseDamage), new DebuffIntent());
        MoveState spinning = new(SpinningMoveId, SpinningMove, new MultiAttackIntent(SpinningDamage, SpinningHits));
        MoveState nightmare = new(NightmareMoveId, NightmareMove, new BuffIntent());

        _curseState = curse;

        ConditionalBranchState sleepBranch = new("SLEEPY_SCARECROW_SLEEP_BRANCH");
        sleepBranch.AddState(sleep, () => Creature.HasPower<StartledDreamPower>());
        sleepBranch.AddState(curse, () => !Creature.HasPower<StartledDreamPower>());

        sleep.FollowUpState = sleepBranch;
        curse.FollowUpState = spinning;
        spinning.FollowUpState = nightmare;
        nightmare.FollowUpState = curse;

        return new MonsterMoveStateMachine([sleepBranch, sleep, curse, spinning, nightmare], sleep);
    }

    private async Task Sleep() {
        IsAwake = false;
        await CreatureCmd.TriggerAnim(Creature, "Sleep", 0f);
        await PowerCmd.Apply<PlatingPower>(Creature, InitialPlating, Creature, null);
        await PowerCmd.Apply<StartledDreamPower>(Creature, SleepTurns, Creature, null);
        Marker2D? sleepVfxPos = NCombatRoom.Instance?.GetCreatureNode(Creature)?.GetSpecialNode<Marker2D>("%SleepVfxPos");
        if (sleepVfxPos == null) {
            return;
        }

        SleepingVfx = NSleepingVfx.Create(sleepVfxPos.GlobalPosition);
        sleepVfxPos.AddChildSafely(SleepingVfx);
        SleepingVfx.Position = Vector2.Zero;
    }

    private Task SleepMove(IReadOnlyList<Creature> targets) {
        _ = targets;
        return Task.CompletedTask;
    }

    private async Task CurseMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(CurseDamage)
            .FromMonster(this)
            .WithAttackerAnim("AttackHeavy", 0.3f)
            .WithAttackerFx(null, SlamSfx)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);

        await PowerCmd.Apply<WeakPower>(LivingPlayerTargets(targets), CurseWeak, Creature, null);
    }

    private async Task SpinningMove(IReadOnlyList<Creature> targets) {
        _ = targets;
        await DamageCmd.Attack(SpinningDamage)
            .FromMonster(this)
            .WithHitCount(SpinningHits)
            .OnlyPlayAnimOnce()
            .WithAttackerAnim("AttackDouble", 0.15f)
            .WithAttackerFx(null, StabSfx)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
    }

    private async Task NightmareMove(IReadOnlyList<Creature> targets) {
        _ = targets;
        SfxCmd.Play(CastMoveSfx);
        await CreatureCmd.TriggerAnim(Creature, "Cast", 0.6f);
        await PowerCmd.Apply<StrengthPower>(Creature, NightmareStrength, Creature, null);
    }

    private void StopSleepingVfx() {
        SleepingVfx?.Stop();
        SleepingVfx = null;
    }

    private static IReadOnlyList<Creature> LivingPlayerTargets(IReadOnlyList<Creature> targets) {
        return targets.Where(static target => target.IsAlive && target.IsPlayer).ToList();
    }

    public override CreatureAnimator SetupCustomAnimationStates(MegaSprite controller) {
        AnimState sleep = new("sleep_loop", isLooping: true);
        AnimState hurtSleeping = new("hurt_sleeping");
        AnimState wake = new("wake_up");
        AnimState idle = new("idle_loop", isLooping: true);
        AnimState cast = new("cast");
        AnimState attackHeavy = new("attack_heavy");
        AnimState attackDouble = new("attack_double");
        AnimState hurt = new("hurt");
        AnimState die = new("die");

        hurtSleeping.NextState = wake;
        wake.NextState = idle;
        cast.NextState = idle;
        attackHeavy.NextState = idle;
        attackDouble.NextState = idle;
        hurt.NextState = idle;

        CreatureAnimator animator = new(idle, controller);
        animator.AddAnyState("Sleep", sleep);
        animator.AddAnyState("Wake", wake, () => !IsAwake);
        animator.AddAnyState("Cast", cast);
        animator.AddAnyState("AttackHeavy", attackHeavy);
        animator.AddAnyState("AttackDouble", attackDouble);
        animator.AddAnyState("Dead", die);
        animator.AddAnyState("Hit", hurt, () => IsAwake);
        animator.AddAnyState("Hit", hurtSleeping, () => !IsAwake);
        return animator;
    }

}
