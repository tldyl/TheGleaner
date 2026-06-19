using BaseLib.Abstracts;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace DemoMod.TheGleaner.Monsters;

public class ScrutinyDoor : CustomMonsterModel {
    public override int MinInitialHp => 220;
    public override int MaxInitialHp => 220;
    public override string CustomVisualPath => "res://TheGleaner/scenes/monsters/portals_boss/scrutiny_door.tscn";

    private int ClawDamage => 6;
    private int PunchDamage => 6;
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        List<MonsterState> states = [];
        MoveState initialState = new MoveState("STATUS_MOVE", StatusMove, new SingleAttackIntent(ClawDamage), new StatusIntent(2));
        MoveState punchState = new MoveState("PUNCH_MOVE", PunchMove, new SingleAttackIntent(PunchDamage), new DebuffIntent());
        RandomBranchState randomBranchState = new RandomBranchState("RAND_MOVE");
        randomBranchState.AddBranch(initialState, MoveRepeatType.CannotRepeat);
        randomBranchState.AddBranch(punchState, 2);
        initialState.FollowUpState = randomBranchState;
        punchState.FollowUpState = randomBranchState;
        states.Add(initialState);
        states.Add(punchState);
        states.Add(randomBranchState);
        return new MonsterMoveStateMachine(states, initialState);
    }

    public override async Task AfterAddedToRoom() {
        await PowerCmd.Apply<MakerAppearPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }
    
    private async Task StatusMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(ClawDamage)
            .FromMonster(this).WithAttackerAnim("Attack", 0.2f)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Dazed>(targets, PileType.Draw, 2, null, CardPilePosition.Random);
    }
    
    private async Task PunchMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(PunchDamage)
            .FromMonster(this).WithAttackerAnim("Attack", 0.2f)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, 2, Creature, null);
    }
}
