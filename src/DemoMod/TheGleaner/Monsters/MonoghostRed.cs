using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace DemoMod.TheGleaner.Monsters;

public class MonoghostRed : CustomMonsterModel {
    public override int MinInitialHp => 20;
    public override int MaxInitialHp => 20;
    
    public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/leaf_slime_m");
    public override string CustomAttackSfx => "event:/sfx/enemy/enemy_attacks/leaf_slime_m/leaf_slime_m_attack";
    public override string CustomCastSfx => "event:/sfx/enemy/enemy_attacks/leaf_slime_m/leaf_slime_m_cast";
    
    private int AttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int SearDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        List<MonsterState> states = [];
        MoveState initialState = new MoveState("ATTACK_MOVE", AttackMove, new SingleAttackIntent(AttackDamage));
        MoveState searState = new MoveState("SEAR_MOVE", SearMove, new SingleAttackIntent(SearDamage), new StatusIntent(1));
        initialState.FollowUpState = searState;
        searState.FollowUpState = initialState;
        states.Add(initialState);
        states.Add(searState);
        return new MonsterMoveStateMachine(states, initialState);
    }

    private async Task AttackMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(AttackDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.15f)
            .WithAttackerFx(null, CustomAttackSfx)
            .WithHitFx("vfx/vfx_slime_impact")
            .Execute(null);
    }
    
    private async Task SearMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(SearDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.15f)
            .WithAttackerFx(null, CustomAttackSfx)
            .WithHitFx("vfx/vfx_slime_impact")
            .Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Burn>(targets, PileType.Hand, 1, null);
    }
}
