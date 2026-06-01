using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace DemoMod.TheGleaner.Monsters;

public class Dimlight : CustomMonsterModel {
    public override int MinInitialHp => 20;
    public override int MaxInitialHp => 20;
    
    public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/parafright");
    protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/obscura/obscura_hologram_attack";
    public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/obscura/obscura_hologram_die";

    public override bool HasDeathSfx => false;
    public override bool ShouldDisappearFromDoom => false;

    public override async Task AfterAddedToRoom() {
        await PowerCmd.Apply<IllusionPower>(Creature, 1M, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        List<MonsterState> states = [];
        MoveState initialState = new MoveState("HEAT_UP_MOVE", HeatUpMove, new DebuffIntent(true));
        initialState.FollowUpState = initialState;
        states.Add(initialState);
        return new MonsterMoveStateMachine(states, initialState);
    }

    private async Task HeatUpMove(IReadOnlyList<Creature> targets) {
        foreach (Creature target in targets) {
            if (target.IsPlayer) {
                foreach (CardModel card in target.Player.PlayerCombatState.AllCards.Where(c => c is Burn)) {
                    card.DynamicVars.Damage.BaseValue += 2;
                }
            }
        }
    }
}
