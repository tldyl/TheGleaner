using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace DemoMod.TheGleaner.Monsters;

public class GraspDoor : CustomMonsterModel {
    public override int MinInitialHp => 220;
    public override int MaxInitialHp => 220;
    public override string CustomVisualPath => "res://TheGleaner/scenes/monsters/portals_boss/grasp_door.tscn";

    private int ClawDamage => 6;
    private int PunchDamage => 5;
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        List<MonsterState> states = [];
        MoveState initialState = new MoveState("STATUS_MOVE", StatusMove, new SingleAttackIntent(ClawDamage), new BuffIntent());
        MoveState punchState = new MoveState("PUNCH_MOVE", PunchMove, new MultiAttackIntent(PunchDamage, 2));
        MoveState defendState = new MoveState("DEFEND_MOVE", DefendMove, new BuffIntent());
        RandomBranchState randomBranchState = new RandomBranchState("RAND_MOVE");
        randomBranchState.AddBranch(initialState, MoveRepeatType.CannotRepeat);
        randomBranchState.AddBranch(punchState, MoveRepeatType.CannotRepeat);
        randomBranchState.AddBranch(defendState, MoveRepeatType.CannotRepeat);
        initialState.FollowUpState = randomBranchState;
        punchState.FollowUpState = randomBranchState;
        defendState.FollowUpState = randomBranchState;
        states.Add(initialState);
        states.Add(punchState);
        states.Add(defendState);
        states.Add(randomBranchState);
        return new MonsterMoveStateMachine(states, initialState);
    }
    
    private async Task StatusMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(ClawDamage)
            .FromMonster(this).WithAttackerAnim("Attack", 0.2f)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
        foreach (Creature creature in Creature.CombatState.HittableEnemies) {
            await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), creature, 5, Creature, null);
        }
    }
    
    private async Task PunchMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(PunchDamage)
            .FromMonster(this).WithAttackerAnim("Attack", 0.2f)
            .WithHitCount(2)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
    }

    private async Task DefendMove(IReadOnlyList<Creature> targets) {
        foreach (Creature creature in Creature.CombatState.HittableEnemies) {
            await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), creature, 10, Creature, null);
        }
    }
}
