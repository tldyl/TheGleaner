using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace DemoMod.TheGleaner.Monsters;

public class HungerDoor : CustomMonsterModel {
    public override int MinInitialHp => 220;
    public override int MaxInitialHp => 220;
    public override string CustomVisualPath => "res://TheGleaner/scenes/monsters/portals_boss/hunger_door.tscn";

    private int ClawDamage => 2;
    private int PunchDamage => 12;
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        List<MonsterState> states = [];
        MoveState initialState = new MoveState("STATUS_MOVE", StatusMove, new MultiAttackIntent(ClawDamage, 3));
        MoveState punchState = new MoveState("PUNCH_MOVE", PunchMove, new SingleAttackIntent(PunchDamage));
        RandomBranchState randomBranchState = new RandomBranchState("RAND_MOVE");
        randomBranchState.AddBranch(initialState, 3);
        randomBranchState.AddBranch(punchState, 3);
        initialState.FollowUpState = randomBranchState;
        punchState.FollowUpState = randomBranchState;
        states.Add(initialState);
        states.Add(punchState);
        states.Add(randomBranchState);
        return new MonsterMoveStateMachine(states, initialState);
    }
    
    private async Task StatusMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(ClawDamage)
            .FromMonster(this).WithAttackerAnim("Attack", 0.2f)
            .WithHitCount(3)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
    }
    
    private async Task PunchMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(PunchDamage)
            .FromMonster(this).WithAttackerAnim("Attack", 0.2f)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
    }
}
