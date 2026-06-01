using BaseLib.Abstracts;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace DemoMod.TheGleaner.Monsters;

public class SummonedTransient : CustomMonsterModel {
    public override int MinInitialHp => 999;
    public override int MaxInitialHp => 999;
    
    public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/leaf_slime_m");
    
    private int BashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 20, 15);

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        List<MonsterState> states = [];
        MoveState initialState = new MoveState("ATTACK_MOVE", AttackMove, new SingleAttackIntent(BashDamage));
        initialState.FollowUpState = initialState;
        states.Add(initialState);
        return new MonsterMoveStateMachine(states, initialState);
    }

    public override async Task AfterAddedToRoom() {
        await PowerCmd.Apply<MinionPower>(Creature, 1, Creature, null);
        await PowerCmd.Apply<ShiftingPower>(Creature, 1, Creature, null);
    }
    
    private async Task AttackMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(BashDamage)
            .FromMonster(this)
            //.WithAttackerAnim("Attack", 0.5f)
            //.WithAttackerFx(null, CustomAttackSfx)
            .Execute(null);
    }
}
