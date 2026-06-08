using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using DemoMod.TheGleaner.Monsters;

namespace DemoMod.TheGleaner.Powers;

public class SorceressBoundPower : CustomPowerModel {
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;
    
    public override bool ShouldOwnerDeathTriggerFatal() => false;

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength) {
        if (creature.Monster is ZapfliSorceress) {
            Owner.Monster.SetMoveImmediate(new MoveState("ESCAPE_MOVE", EscapeMove, new EscapeIntent()));
            await PowerCmd.Remove(this);
            await Owner.Monster.NextMove.PerformMove([]);
        }
    }

    private async Task EscapeMove(IReadOnlyList<Creature> targets) {
        await CreatureCmd.Escape(Owner);
    }
}
