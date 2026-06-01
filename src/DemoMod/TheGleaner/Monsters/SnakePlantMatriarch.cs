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

public class SnakePlantMatriarch : CustomMonsterModel {
    public override int MinInitialHp => 110;
    public override int MaxInitialHp => 110;
    
    public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/hunter_killer");
    
    private int BashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 8);

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        List<MonsterState> states = [];
        MoveState initialState = new MoveState("BITE_MOVE", BiteMove, new MultiAttackIntent(BashDamage, 3));
        MoveState weakSporeState = new MoveState("WEAK_SPORE_MOVE", WeakSporeMove, new DebuffIntent(true));
        MoveState biteState = new MoveState("BITE_AGAIN_MOVE", BiteMove, new MultiAttackIntent(BashDamage, 3));
        initialState.FollowUpState = weakSporeState;
        weakSporeState.FollowUpState = biteState;
        biteState.FollowUpState = biteState;
        states.Add(initialState);
        states.Add(weakSporeState);
        states.Add(biteState);
        return new MonsterMoveStateMachine(states, initialState);
    }

    public override async Task AfterAddedToRoom() {
        await PowerCmd.Apply<MalleablePower>(Creature, 3, Creature, null);
        await PowerCmd.Apply<HardToKillPower>(Creature, 15, Creature, null);
    }

    private async Task BiteMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(BashDamage)
            .FromMonster(this)
            //.WithAttackerAnim("Attack", 0.5f)
            //.WithAttackerFx(null, CustomAttackSfx)
            .WithHitFx("vfx/vfx_bite")
            .WithHitCount(3)
            .Execute(null);
    }
    
    private async Task WeakSporeMove(IReadOnlyList<Creature> targets) {
        await PowerCmd.Apply<WeakPower>(targets, 99, Creature, null);
        await PowerCmd.Apply<FrailPower>(targets, 99, Creature, null);
    }
}
