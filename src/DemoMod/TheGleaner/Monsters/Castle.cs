using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Monsters;
public class Castle : CustomMonsterModel {
    public const string IronCurtainBaptismMoveId = "IRON_CURTAIN_BAPTISM_MOVE";

    private const int InitialHp = 70;
    private const int IronCurtainDamage = 15;
    private const int IronCurtainBlock = 10;
    private const string LivingShieldAttackSfx = "event:/sfx/enemy/enemy_attacks/living_shield/living_shield_attack";

    public override int MinInitialHp => InitialHp;

    public override int MaxInitialHp => InitialHp;

    public override bool HasDeathSfx => false;

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

    public override string CustomAttackSfx => LivingShieldAttackSfx;

    public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/living_shield");

    public override async Task AfterAddedToRoom() {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<MinionPower>(Creature, 1m, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        MoveState ironCurtainBaptism = new(
            IronCurtainBaptismMoveId,
            IronCurtainBaptismMove,
            new SingleAttackIntent(IronCurtainDamage),
            new DefendIntent());

        ironCurtainBaptism.FollowUpState = ironCurtainBaptism;
        return new MonsterMoveStateMachine([ironCurtainBaptism], ironCurtainBaptism);
    }

    private async Task IronCurtainBaptismMove(IReadOnlyList<Creature> targets) {
        _ = targets;
        await DamageCmd.Attack(IronCurtainDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.3f)
            .WithAttackerFx(null, CustomAttackSfx)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);

        foreach (Creature creature in CombatState.Creatures.Where(creature => creature.IsAlive && creature != Creature)) {
            await CreatureCmd.GainBlock(creature, IronCurtainBlock, ValueProp.Move, null);
        }
    }
}

