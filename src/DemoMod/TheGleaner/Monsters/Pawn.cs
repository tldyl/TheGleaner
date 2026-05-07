using BaseLib.Abstracts;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace DemoMod.TheGleaner.Monsters;

public class Pawn : CustomMonsterModel {
    public const string AdvanceMoveId = "ADVANCE_MOVE";
    public const string DeterrenceMoveId = "DETERRENCE_MOVE";

    private const int InitialHp = 45;
    private const int AdvanceDamage = 10;
    private const int DeterrenceDamage = 5;
    private const int DeterrenceWeak = 1;
    private const int PromotionActions = 3;
    private const int PromotionStrength = 20;
    private const string TurretOperatorAttackSfx = "event:/sfx/enemy/enemy_attacks/turret_operator/turret_operator_attack";

    public override int MinInitialHp => InitialHp;
    public override int MaxInitialHp => InitialHp;

    public override string HurtSfx => "event:/sfx/enemy/enemy_attacks/turret_operator/turret_operator_hurt";

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

    public override string CustomAttackSfx => TurretOperatorAttackSfx;

    public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/turret_operator");

    public override async Task AfterAddedToRoom() {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<PromotionPower>(Creature, PromotionActions, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        MoveState advance = new(AdvanceMoveId, AdvanceMove, new SingleAttackIntent(AdvanceDamage));
        MoveState deterrence = new(DeterrenceMoveId, DeterrenceMove, new SingleAttackIntent(DeterrenceDamage), new DebuffIntent());

        advance.FollowUpState = deterrence;
        deterrence.FollowUpState = advance;

        return new MonsterMoveStateMachine([advance, deterrence], advance);
    }

    private async Task AdvanceMove(IReadOnlyList<Creature> targets) {
        _ = targets;
        await DamageCmd.Attack(AdvanceDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.4f)
            .WithAttackerFx(null, CustomAttackSfx)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);
        await AdvancePromotion();
    }

    private async Task DeterrenceMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(DeterrenceDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.4f)
            .WithAttackerFx(null, CustomAttackSfx)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);

        IReadOnlyList<Creature> playerTargets = targets.Where(static target => target.IsAlive && target.IsPlayer).ToList();
        await PowerCmd.Apply<WeakPower>(playerTargets, DeterrenceWeak, Creature, null);
        await AdvancePromotion();
    }

    private async Task AdvancePromotion() {
        PromotionPower? promotion = Creature.GetPower<PromotionPower>();
        if (promotion == null) {
            return;
        }

        promotion.TriggerFlash();
        if (promotion.Amount > 1) {
            await PowerCmd.ModifyAmount(promotion, -1m, Creature, null);
            return;
        }

        await PowerCmd.Remove(promotion);
        await PowerCmd.Apply<StrengthPower>(Creature, PromotionStrength, Creature, null);
    }

    public override CreatureAnimator SetupCustomAnimationStates(MegaSprite controller) {
        AnimState idle = new("idle_loop", isLooping: true);
        AnimState crank = new("crank");
        AnimState attack = new("attack");
        AnimState hurt = new("hurt");
        AnimState die = new("die");
        crank.NextState = idle;
        attack.NextState = idle;
        hurt.NextState = idle;

        CreatureAnimator animator = new(idle, controller);
        animator.AddAnyState("Crank", crank);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Dead", die);
        animator.AddAnyState("Hit", hurt);
        return animator;
    }
}
