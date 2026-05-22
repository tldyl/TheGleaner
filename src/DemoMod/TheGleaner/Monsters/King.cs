using BaseLib.Abstracts;
using DemoMod.TheGleaner.Cards.GleanerCard;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace DemoMod.TheGleaner.Monsters;

public class King : CustomMonsterModel {
    public const string DeclarationOfWarMoveId = "DECLARATION_OF_WAR_MOVE";
    public const string RoyalOppressionMoveId = "ROYAL_OPPRESSION_MOVE";
    public const string RoyalDecreeMoveId = "ROYAL_DECREE_MOVE";
    public const string RoyalRallyMoveId = "ROYAL_RALLY_MOVE";
    public const string CounterattackMoveId = "COUNTERATTACK_MOVE";

    private const int InitialHp = 99;
    private const int DeclarationCheckmateCards = 1;
    private const int RoyalOppressionDamage = 12;
    private const int RoyalOppressionFrail = 1;
    private const int RoyalDecreeDamage = 12;
    private const int RoyalDecreeVulnerable = 1;
    private const int RoyalRallyStrength = 3;
    private const int CounterattackDamage = 9;
    private const string SoulGrenadeSfx = "event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_soul_grenade";
    private const string RallySfx = "event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_rally";
    private const string SoulBeamSfx = "event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_soul_beam";

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int CounterattackUses { get; set; }

    public override int MinInitialHp => InitialHp;
    public override int MaxInitialHp => InitialHp;
    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;
    public override string HurtSfx => "event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_hurt";
    public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_die";
    public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/kin_priest");

    private int CounterattackHits => CounterattackUses + 1;

    public override async Task AfterAddedToRoom() {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<ProtectTheKingPower>(Creature, 1m, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        MoveState declaration = new(
            DeclarationOfWarMoveId,
            DeclarationOfWarMove,
            new StatusIntent(DeclarationCheckmateCards));
        MoveState royalOppression = new(
            RoyalOppressionMoveId,
            RoyalOppressionMove,
            new SingleAttackIntent(RoyalOppressionDamage),
            new DebuffIntent());
        MoveState royalDecree = new(
            RoyalDecreeMoveId,
            RoyalDecreeMove,
            new SingleAttackIntent(RoyalDecreeDamage),
            new DebuffIntent());
        MoveState royalRally = new(
            RoyalRallyMoveId,
            RoyalRallyMove,
            new BuffIntent());
        MoveState counterattack = new(
            CounterattackMoveId,
            CounterattackMove,
            new MultiAttackIntent(CounterattackDamage, () => CounterattackHits));

        RandomBranchState randomOrders = new("KING_RANDOM_ORDERS_BRANCH");
        randomOrders.AddBranch(royalOppression, MoveRepeatType.CannotRepeat);
        randomOrders.AddBranch(royalDecree, MoveRepeatType.CannotRepeat);
        randomOrders.AddBranch(royalRally, MoveRepeatType.CannotRepeat);

        ConditionalBranchState protectedBranch = new("KING_PROTECTED_BRANCH");
        protectedBranch.AddState(counterattack, () => !Creature.HasPower<ProtectTheKingPower>());
        protectedBranch.AddState(randomOrders, () => true);

        declaration.FollowUpState = protectedBranch;
        royalOppression.FollowUpState = protectedBranch;
        royalDecree.FollowUpState = protectedBranch;
        royalRally.FollowUpState = protectedBranch;
        counterattack.FollowUpState = protectedBranch;

        return new MonsterMoveStateMachine(
            [declaration, royalOppression, royalDecree, royalRally, counterattack, randomOrders, protectedBranch],
            declaration);
    }

    private async Task DeclarationOfWarMove(IReadOnlyList<Creature> targets) {
        await RallyAnimation();
        await CardPileCmd.AddToCombatAndPreview<Checkmate>(
            LivingPlayerTargets(targets),
            PileType.Draw,
            DeclarationCheckmateCards,
            addedByPlayer: false,
            CardPilePosition.Random);
    }

    private async Task RoyalOppressionMove(IReadOnlyList<Creature> targets) {
        await AttackWithGrenade(RoyalOppressionDamage);
        await PowerCmd.Apply<FrailPower>(LivingPlayerTargets(targets), RoyalOppressionFrail, Creature, null);
    }

    private async Task RoyalDecreeMove(IReadOnlyList<Creature> targets) {
        await AttackWithGrenade(RoyalDecreeDamage);
        await PowerCmd.Apply<VulnerablePower>(LivingPlayerTargets(targets), RoyalDecreeVulnerable, Creature, null);
    }

    private async Task RoyalRallyMove(IReadOnlyList<Creature> targets) {
        _ = targets;
        await RallyAnimation();
        await PowerCmd.Apply<StrengthPower>(LivingMonsters(), RoyalRallyStrength, Creature, null);
    }

    private async Task CounterattackMove(IReadOnlyList<Creature> targets) {
        _ = targets;
        await DamageCmd.Attack(CounterattackDamage)
            .WithHitCount(CounterattackHits)
            .FromMonster(this)
            .WithAttackerAnim("AttackLaser", 0.4f)
            .WithAttackerFx(null, SoulBeamSfx)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .OnlyPlayAnimOnce()
            .Execute(null);
        CounterattackUses += 1;
    }

    private async Task AttackWithGrenade(int damage) {
        await DamageCmd.Attack(damage)
            .FromMonster(this)
            .WithAttackerAnim("AttackGrenade", 0f)
            .WithAttackerFx(null, SoulGrenadeSfx)
            .WithWaitBeforeHit(1f, 1f)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);
    }

    private async Task RallyAnimation() {
        SfxCmd.Play(RallySfx);
        await CreatureCmd.TriggerAnim(Creature, "Rally", 1f);
    }

    private static IReadOnlyList<Creature> LivingPlayerTargets(IReadOnlyList<Creature> targets) {
        return targets.Where(static target => target.IsAlive && target.IsPlayer).ToList();
    }

    private IReadOnlyList<Creature> LivingMonsters() {
        return CombatState.Creatures.Where(static creature => creature.IsAlive && creature.IsMonster).ToList();
    }

    public override CreatureAnimator SetupCustomAnimationStates(MegaSprite controller) {
        AnimState idle = new("idle_loop", isLooping: true);
        AnimState rally = new("rally");
        AnimState attackGrenade = new("attack_grenade");
        AnimState attackLaser = new("attack_laser");
        AnimState hurt = new("hurt");
        AnimState die = new("die");
        rally.NextState = idle;
        attackGrenade.NextState = idle;
        attackLaser.NextState = idle;
        hurt.NextState = idle;

        CreatureAnimator animator = new(idle, controller);
        animator.AddAnyState("Rally", rally);
        animator.AddAnyState("AttackGrenade", attackGrenade);
        animator.AddAnyState("AttackLaser", attackLaser);
        animator.AddAnyState("Dead", die);
        animator.AddAnyState("Hit", hurt);
        return animator;
    }
}
