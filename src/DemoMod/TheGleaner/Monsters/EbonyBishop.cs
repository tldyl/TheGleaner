using BaseLib.Abstracts;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace DemoMod.TheGleaner.Encounters;

public class EbonyBishop : Bishop {
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

    public override async Task AfterAddedToRoom() {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<BlackSquareDomainPower>(Creature, DomainAmount, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
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

    private async Task PinDownMove(IReadOnlyList<Creature> targets) {
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

    private async Task ProfanationMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(ProfanationDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.15f)
            .WithAttackerFx(null, CustomAttackSfx)
            .WithHitFx("vfx/vfx_slime_impact")
            .Execute(null);

        await PowerCmd.Apply<WeakPower>(LivingPlayerTargets(targets), ProfanationWeak, Creature, null);
    }

    private async Task DarkCrossMove(IReadOnlyList<Creature> targets) {
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
