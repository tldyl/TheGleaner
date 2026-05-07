using DemoMod.TheGleaner.Encounters;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace DemoMod.TheGleaner.Monsters;

public class IvoryBishop : Bishop {
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

    public override async Task AfterAddedToRoom() {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<WhiteSquareDomainPower>(Creature, DomainAmount, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
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

    private async Task LightCrossMove(IReadOnlyList<Creature> targets) {
        _ = targets;
        await DamageCmd.Attack(LightCrossDamage)
            .FromMonster(this)
            .WithHitCount(LightCrossHits)
            .WithAttackerAnim("Attack", 0.15f)
            .WithAttackerFx(null, CustomAttackSfx)
            .WithHitFx("vfx/vfx_slime_impact")
            .Execute(null);
    }

    private async Task ConvictionMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(ConvictionDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.15f)
            .WithAttackerFx(null, CustomAttackSfx)
            .WithHitFx("vfx/vfx_slime_impact")
            .Execute(null);

        await PowerCmd.Apply<VulnerablePower>(LivingPlayerTargets(targets), ConvictionVulnerable, Creature, null);
    }

    private async Task SanctuaryMove(IReadOnlyList<Creature> targets) {
        _ = targets;
        await Cast();
        await ApplyPlatingToBishops(SanctuaryPlating);
    }
}
