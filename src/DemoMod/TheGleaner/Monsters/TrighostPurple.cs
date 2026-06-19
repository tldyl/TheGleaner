using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Monsters;

public class TrighostPurple : CustomMonsterModel {
    public override int MinInitialHp => 43;
    public override int MaxInitialHp => 47;
    
    public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/leaf_slime_m");
    public override string CustomAttackSfx => "event:/sfx/enemy/enemy_attacks/leaf_slime_m/leaf_slime_m_attack";
    public override string CustomCastSfx => "event:/sfx/enemy/enemy_attacks/leaf_slime_m/leaf_slime_m_cast";
    
    private int AttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);
    private int SearDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 6);

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        List<MonsterState> states = [];
        MoveState initialState = new MoveState("SEAR_MOVE", SearMove, new SingleAttackIntent(SearDamage), new StatusIntent(1));
        MoveState attackState = new MoveState("ATTACK_MOVE", AttackMove, new MultiAttackIntent(AttackDamage, 2));
        MoveState inflameState = new MoveState("INFLAME_MOVE", InflameMove, new BuffIntent(), new DefendIntent());
        MoveState attackAndSearState = new MoveState("ATTACK_AND_SEAR_MOVE", AttackAndSearMove, new MultiAttackIntent(AttackDamage, 2), new StatusIntent(1));
        initialState.FollowUpState = attackState;
        attackState.FollowUpState = inflameState;
        inflameState.FollowUpState = attackAndSearState;
        attackAndSearState.FollowUpState = initialState;
        states.Add(initialState);
        states.Add(attackState);
        states.Add(inflameState);
        states.Add(attackAndSearState);
        return new MonsterMoveStateMachine(states, initialState);
    }

    private async Task InflameMove(IReadOnlyList<Creature> targets) {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 2, Creature, null);
        await CreatureCmd.GainBlock(Creature, 14, ValueProp.Move, null);
    }
    
    private async Task AttackAndSearMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(AttackDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.15f)
            .WithAttackerFx(null, CustomAttackSfx)
            .WithHitFx("vfx/vfx_slime_impact")
            .WithHitCount(2)
            .Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Burn>(targets, PileType.Draw, 1, null);
        foreach (Creature target in targets) {
            if (target.IsPlayer) {
                foreach (CardModel card in target.Player.PlayerCombatState.AllCards.Where(c => c is Burn)) {
                    card.DynamicVars.Damage.BaseValue += 2;
                }
            }
        }
    }
    
    private async Task AttackMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(AttackDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.15f)
            .WithAttackerFx(null, CustomAttackSfx)
            .WithHitFx("vfx/vfx_slime_impact")
            .WithHitCount(2)
            .Execute(null);
    }
    
    private async Task SearMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(SearDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.15f)
            .WithAttackerFx(null, CustomAttackSfx)
            .WithHitFx("vfx/vfx_slime_impact")
            .Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Burn>(targets, PileType.Draw, 1, null);
    }
}
