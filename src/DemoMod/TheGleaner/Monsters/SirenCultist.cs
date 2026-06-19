using BaseLib.Abstracts;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace DemoMod.TheGleaner.Monsters;

public class SirenCultist : CustomMonsterModel {
    public override int MinInitialHp => 85;
    public override int MaxInitialHp => 85;
    public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/calcified_cultist");
    
    private float _attackSfxStrength;
    protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/cultists/cultists_attack";
    private float AttackSfxStrength {
        get => _attackSfxStrength;
        set {
            AssertMutable();
            _attackSfxStrength = value;
        }
    }
    
    private int AttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        List<MonsterState> states = [];
        MoveState initialState = new MoveState("BUFF_MOVE", BuffMove, new BuffIntent(), new DebuffIntent(true));
        MoveState attackState = new MoveState("ATTACK_MOVE", AttackMove, new SingleAttackIntent(AttackDamage));
        initialState.FollowUpState = attackState;
        attackState.FollowUpState = attackState;
        states.Add(initialState);
        states.Add(attackState);
        return new MonsterMoveStateMachine(states, initialState);
    }

    private async Task BuffMove(IReadOnlyList<Creature> targets) {
        SfxCmd.Play("event:/sfx/enemy/enemy_attacks/cultists/cultists_buff_calcified");
        await CreatureCmd.TriggerAnim(Creature, "Cast", 0.5f);
        TalkCmd.Play(new LocString("monsters", "CALCIFIED_CULTIST.moves.INCANTATION.banter"), Creature, VfxColor.Purple, VfxDuration.Standard);
        await Cmd.CustomScaledWait(0.25f, 0.5f);
        await PowerCmd.Apply<RitualPower>(new ThrowingPlayerChoiceContext(), Creature, 2, Creature, null);
        await PowerCmd.Apply<TalkativePower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
    }
    
    private async Task AttackMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(AttackDamage)
            .FromMonster(this).WithAttackerAnim("Attack", 0.2f)
            .BeforeDamage(PlayAttackSfx)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
    }
    
    private Task PlayAttackSfx() {
        SfxCmd.Play(AttackSfx, "enemy_strength", AttackSfxStrength);
        AttackSfxStrength += 0.2f;
        return Task.CompletedTask;
    }
}
