using BaseLib.Abstracts;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace DemoMod.TheGleaner.Monsters;

public class Zapfli : CustomMonsterModel {
    public override int MinInitialHp => 12;
    public override int MaxInitialHp => 14;
    
    public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/noisebot");
    
    private int LightBluntDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 5);

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        List<MonsterState> states = [];
        MoveState initialState = new MoveState("LIGHT_BLUNT_MOVE", LightBluntMove, new SingleAttackIntent(LightBluntDamage));
        initialState.FollowUpState = initialState;
        states.Add(initialState);
        return new MonsterMoveStateMachine(states, initialState);
    }

    public override async Task AfterAddedToRoom() {
        await PowerCmd.Apply<ChargeAppendPower>(Creature, 1, Creature, null);
    }
    
    private async Task LightBluntMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(LightBluntDamage)
            .FromMonster(this)
            .Execute(null);
    }
}
