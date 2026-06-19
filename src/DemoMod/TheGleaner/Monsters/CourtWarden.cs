using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace DemoMod.TheGleaner.Monsters;

public class CourtWarden : CustomMonsterModel {
    public override int MinInitialHp => 80;
    public override int MaxInitialHp => 80;
    
    public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/twig_slime_m");

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        List<MonsterState> states = [];
        MoveState initialState = new MoveState("SUMMON_MOVE", SummonMove, new SummonIntent());
        MoveState buffState = new MoveState("BUFF_MOVE", BuffMove, new BuffIntent());
        initialState.FollowUpState = buffState;
        buffState.FollowUpState = buffState;
        MoveState escapeState = new MoveState("ESCAPE_MOVE", EscapeMove, new EscapeIntent());
        escapeState.FollowUpState = escapeState;
        states.Add(initialState);
        states.Add(buffState);
        states.Add(escapeState);
        return new MonsterMoveStateMachine(states, initialState);
    }

    public override async Task BeforeDeath(Creature creature) {
        if (creature is {IsMonster: true, Monster: SummonedTransient}) {
            Creature.Monster.SetMoveImmediate(new MoveState("ESCAPE_MOVE", EscapeMove, new EscapeIntent()), true);
        }
    }
    
    private async Task SummonMove(IReadOnlyList<Creature> targets) {
        await CreatureCmd.Add<SummonedTransient>(CombatState);
    }

    private async Task BuffMove(IReadOnlyList<Creature> targets) {
        foreach (Creature creature in CombatState.GetTeammatesOf(Creature)) {
            if (creature.Monster is SummonedTransient) {
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), creature, 7, Creature, null);
            }
        }
    }
    
    private async Task EscapeMove(IReadOnlyList<Creature> targets) {
        await CreatureCmd.Escape(Creature);
    }
}
