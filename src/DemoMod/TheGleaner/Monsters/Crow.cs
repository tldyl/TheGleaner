using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace DemoMod.TheGleaner.Monsters;

public class Crow : CustomMonsterModel {
    public const string PeckMoveId = "PECK_MOVE";
    public const string CawMoveId = "CAW_MOVE";
    public const string SwoopMoveId = "SWOOP_MOVE";
    public const string HeadbuttMoveId = "HEADBUTT_MOVE";
    public const string FlyMoveId = "FLY_MOVE";

    private const int InitialHp = 55;
    private const int PeckDamage = 2;
    private const int PeckHits = 5;
    private const int CawStrength = 1;
    private const int SwoopDamage = 12;
    private const int HeadbuttDamage = 6;
    private const string GroundedAttackSfx = "event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_attack";
    private const string HoverAttackSfx = "event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_attack_hover";
    private const string TakeOffSfx = "event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_take_off";
    private const string HoverLoopSfx = "event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_hover_loop";
    private const string HurtHoverSfx = "event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_hurt_hover";

    private bool _isHovering;

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int CoordinatedActionSlot { get; set; }

    public override int MinInitialHp => InitialHp;

    public override int MaxInitialHp => InitialHp;

    public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/thieving_hopper");

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Insect;

    public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_die";

    public override string TakeDamageSfx => IsHovering ? HurtHoverSfx : base.TakeDamageSfx;

    public bool IsHovering {
        get => _isHovering;
        private set {
            AssertMutable();
            _isHovering = value;
        }
    }

    protected override string AttackSfx => IsHovering ? HoverAttackSfx : GroundedAttackSfx;

    public override async Task AfterAddedToRoom() {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<SoarPower>(Creature, 1m, Creature, null);
        await StartHover(playTakeOffSfx: false, waitForAnim: false);
    }

    public override void BeforeRemovedFromRoom() {
        SfxCmd.StopLoop(HoverLoopSfx);
    }

    public async Task StartledByScarecrow() {
        if (Creature.IsDead) {
            return;
        }

        if (Creature.GetPower<SoarPower>() is { } soar) {
            await PowerCmd.Remove(soar);
        }
        StopHover();
        await CreatureCmd.Stun(Creature, StunnedByScarecrowMove, HeadbuttMoveId);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        MoveState peck = new(PeckMoveId, PeckMove, new MultiAttackIntent(PeckDamage, PeckHits));
        MoveState caw = new(CawMoveId, CawMove, new BuffIntent());
        MoveState swoop = new(SwoopMoveId, SwoopMove, new SingleAttackIntent(SwoopDamage));
        MoveState headbutt = new(HeadbuttMoveId, HeadbuttMove, new SingleAttackIntent(HeadbuttDamage));
        MoveState fly = new(FlyMoveId, FlyMove, new BuffIntent());

        ConditionalBranchState opening = new("CROW_OPENING_BRANCH");
        opening.AddState(peck, () => NormalizedActionSlot == 0);
        opening.AddState(caw, () => NormalizedActionSlot == 1);
        opening.AddState(swoop, () => NormalizedActionSlot == 2);

        ConditionalBranchState afterFly = new("CROW_AFTER_FLY_BRANCH");
        afterFly.AddState(peck, () => NormalizedActionSlot == 0);
        afterFly.AddState(caw, () => NormalizedActionSlot == 1);
        afterFly.AddState(swoop, () => NormalizedActionSlot == 2);

        RandomBranchState random = new("CROW_RANDOM_BRANCH");
        random.AddBranch(peck, MoveRepeatType.CannotRepeat);
        random.AddBranch(caw, MoveRepeatType.CannotRepeat);
        random.AddBranch(swoop, MoveRepeatType.CannotRepeat);

        peck.FollowUpState = random;
        caw.FollowUpState = random;
        swoop.FollowUpState = random;
        headbutt.FollowUpState = fly;
        fly.FollowUpState = afterFly;

        return new MonsterMoveStateMachine([opening, afterFly, random, peck, caw, swoop, headbutt, fly], opening);
    }

    private int NormalizedActionSlot => ((CoordinatedActionSlot % 3) + 3) % 3;

    private async Task PeckMove(IReadOnlyList<Creature> targets) {
        _ = targets;
        await DamageCmd.Attack(PeckDamage)
            .FromMonster(this)
            .WithHitCount(PeckHits)
            .OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.2f)
            .WithAttackerFx(null, AttackSfx)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
    }

    private async Task CawMove(IReadOnlyList<Creature> targets) {
        _ = targets;
        SfxCmd.Play(TakeOffSfx);
        await CreatureCmd.TriggerAnim(Creature, "Attack", 0.3f);
        await PowerCmd.Apply<StrengthPower>(Creature, CawStrength, Creature, null);
    }

    private async Task SwoopMove(IReadOnlyList<Creature> targets) {
        _ = targets;
        await DamageCmd.Attack(SwoopDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.25f)
            .WithAttackerFx(null, AttackSfx)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
    }

    private async Task HeadbuttMove(IReadOnlyList<Creature> targets) {
        _ = targets;
        await DamageCmd.Attack(HeadbuttDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.25f)
            .WithAttackerFx(null, GroundedAttackSfx)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);
    }

    private async Task FlyMove(IReadOnlyList<Creature> targets) {
        _ = targets;
        await StartHover(playTakeOffSfx: true, waitForAnim: true);
        await PowerCmd.Apply<SoarPower>(Creature, 1m, Creature, null);
    }

    private async Task StunnedByScarecrowMove(IReadOnlyList<Creature> targets) {
        _ = targets;
        await CreatureCmd.TriggerAnim(Creature, "StunTrigger", 0.2f);
    }

    private async Task StartHover(bool playTakeOffSfx, bool waitForAnim) {
        if (IsHovering) {
            return;
        }

        IsHovering = true;
        if (playTakeOffSfx) {
            SfxCmd.Play(TakeOffSfx);
        }
        SfxCmd.PlayLoop(HoverLoopSfx);
        await CreatureCmd.TriggerAnim(Creature, "Hover", 0f);
        if (waitForAnim) {
            await Cmd.Wait(1.25f);
        }
    }

    private void StopHover() {
        if (!IsHovering) {
            return;
        }

        SfxCmd.StopLoop(HoverLoopSfx);
        IsHovering = false;
    }

    public override CreatureAnimator SetupCustomAnimationStates(MegaSprite controller) {
        AnimState groundedIdle = new("idle_loop", isLooping: true) {
            BoundsContainer = "GroundedBounds"
        };
        AnimState hurt = new("hurt") {
            NextState = groundedIdle
        };
        AnimState hurtHover = new("hurt_hover");
        AnimState attack = new("attack") {
            NextState = groundedIdle
        };
        AnimState attackHover = new("attack_hover");
        AnimState die = new("die");
        AnimState takeOff = new("take_off");
        AnimState hoverLoop = new("hover_loop", isLooping: true) {
            BoundsContainer = "FlyingBounds"
        };

        hurtHover.NextState = hoverLoop;
        attackHover.NextState = hoverLoop;
        takeOff.NextState = hoverLoop;

        CreatureAnimator animator = new(groundedIdle, controller);
        animator.AddAnyState("StunTrigger", groundedIdle);
        animator.AddAnyState("Hover", takeOff);
        animator.AddAnyState("Dead", die);
        animator.AddAnyState("Hit", hurtHover, () => IsHovering);
        animator.AddAnyState("Hit", hurt, () => !IsHovering);
        animator.AddAnyState("Attack", attackHover, () => IsHovering);
        animator.AddAnyState("Attack", attack, () => !IsHovering);
        return animator;
    }
}
