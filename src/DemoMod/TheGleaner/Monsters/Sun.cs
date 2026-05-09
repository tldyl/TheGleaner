using BaseLib.Abstracts;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace DemoMod.TheGleaner.Monsters;

public class Sun : CustomMonsterModel {
    public const string ProminenceMoveId = "PROMINENCE_MOVE";
    public const string SolarFocusMoveId = "SOLAR_FOCUS_MOVE";
    public const string SunstormMoveId = "SUNSTORM_MOVE";
    public const string LingeringHeatMoveId = "LINGERING_HEAT_MOVE";

    private const int InitialHp = 220;
    private const int ProminenceBurnsPerPile = 3;
    private const int SolarFocusDamage = 16;
    private const int SolarFocusBurns = 1;
    private const int SunstormDamage = 4;
    private const int SunstormHits = 6;
    private const int LingeringHeatBurnsPerPile = 2;
    private const string PhrogParasiteAttackSfx = "event:/sfx/enemy/enemy_attacks/phrog_parasite/phrog_parasite_attack";

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool HasUsedLingeringHeat { get; set; }

    public override int MinInitialHp => InitialHp;

    public override int MaxInitialHp => InitialHp;

    public override string HurtSfx => "event:/sfx/enemy/enemy_attacks/phrog_parasite/phrog_parasite_hurt";

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

    public override string CustomAttackSfx => PhrogParasiteAttackSfx;

    public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/phrog_parasite");

    public override async Task AfterAddedToRoom() {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<BakingPower>(Creature, 1m, Creature, null);
        await PowerCmd.Apply<SearingScaldsPower>(Creature, 1m, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        MoveState prominence = new(
            ProminenceMoveId,
            ProminenceMove,
            new StatusIntent(ProminenceBurnsPerPile * 2));
        MoveState solarFocus = new(
            SolarFocusMoveId,
            SolarFocusMove,
            new SingleAttackIntent(SolarFocusDamage),
            new StatusIntent(SolarFocusBurns));
        MoveState sunstorm = new(
            SunstormMoveId,
            SunstormMove,
            new MultiAttackIntent(SunstormDamage, SunstormHits));
        MoveState lingeringHeat = new(
            LingeringHeatMoveId,
            LingeringHeatMove,
            new StatusIntent(LingeringHeatBurnsPerPile * 2));

        RandomBranchState loopStart = new("SUN_LOOP_START");
        loopStart.AddBranch(solarFocus, MoveRepeatType.CanRepeatForever);
        loopStart.AddBranch(sunstorm, MoveRepeatType.CanRepeatForever);

        ConditionalBranchState afterSunstorm = new("SUN_AFTER_SUNSTORM");
        afterSunstorm.AddState(lingeringHeat, () => !HasUsedLingeringHeat);
        afterSunstorm.AddState(solarFocus, () => true);

        prominence.FollowUpState = solarFocus;
        solarFocus.FollowUpState = sunstorm;
        sunstorm.FollowUpState = afterSunstorm;
        lingeringHeat.FollowUpState = loopStart;

        return new MonsterMoveStateMachine(
            [prominence, solarFocus, sunstorm, lingeringHeat, loopStart, afterSunstorm],
            prominence);
    }

    private async Task ProminenceMove(IReadOnlyList<Creature> targets) {
        await PlayStatusAnimation();
        foreach (Creature target in LivingPlayerTargets(targets)) {
            await AddBurnsToDrawAndDiscard(target, ProminenceBurnsPerPile);
        }
    }

    private async Task SolarFocusMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(SolarFocusDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.3f)
            .WithAttackerFx(null, CustomAttackSfx)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);

        await CardPileCmd.AddToCombatAndPreview<Burn>(
            LivingPlayerTargets(targets),
            PileType.Hand,
            SolarFocusBurns,
            addedByPlayer: false);
    }

    private async Task SunstormMove(IReadOnlyList<Creature> targets) {
        _ = targets;
        await DamageCmd.Attack(SunstormDamage)
            .FromMonster(this)
            .WithHitCount(SunstormHits)
            .OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.15f)
            .WithAttackerFx(null, CustomAttackSfx)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);
    }

    private async Task LingeringHeatMove(IReadOnlyList<Creature> targets) {
        await PlayStatusAnimation();
        foreach (Creature target in LivingPlayerTargets(targets)) {
            await AddBurnsToDrawAndDiscard(target, LingeringHeatBurnsPerPile);
        }
        HasUsedLingeringHeat = true;
    }

    private async Task PlayStatusAnimation() {
        SfxCmd.Play(CustomAttackSfx);
        await CreatureCmd.TriggerAnim(Creature, "Attack", 0.35f);
    }

    private static IReadOnlyList<Creature> LivingPlayerTargets(IReadOnlyList<Creature> targets) {
        return targets.Where(static target => target.IsAlive && target.IsPlayer).ToList();
    }

    private static async Task AddBurnsToDrawAndDiscard(Creature target, int count) {
        await CardPileCmd.AddToCombatAndPreview<Burn>(
            target,
            PileType.Draw,
            count,
            addedByPlayer: false,
            CardPilePosition.Random);
        await CardPileCmd.AddToCombatAndPreview<Burn>(
            target,
            PileType.Discard,
            count,
            addedByPlayer: false,
            CardPilePosition.Random);
    }
}
