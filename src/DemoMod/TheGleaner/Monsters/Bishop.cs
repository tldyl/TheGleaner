using BaseLib.Abstracts;
using DemoMod.TheGleaner.Monsters;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Encounters;
public abstract class Bishop : CustomMonsterModel {
    protected const int DomainAmount = 1;
    protected const int SanctuaryPlating = 4;

    private const int InitialHp = 180;
    private const int InitialPlating = 10;

    public override int MinInitialHp => InitialHp;

    public override int MaxInitialHp => InitialHp;

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Slime;

    public override async Task AfterAddedToRoom() {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<PlatingPower>(Creature, InitialPlating, Creature, null);
        await PowerCmd.Apply<FanaticPower>(Creature, 1m, Creature, null);
    }

    protected static IReadOnlyList<Creature> LivingPlayerTargets(IReadOnlyList<Creature> targets) {
        return targets.Where(static target => target.IsAlive && target.IsPlayer).ToList();
    }

    protected async Task ApplyPlatingToBishops(int amount) {
        foreach (Creature creature in CombatState.Creatures.Where(IsLivingBishop)) {
            await PowerCmd.Apply<PlatingPower>(creature, amount, Creature, null);
        }
    }

    protected async Task Cast() {
        SfxCmd.Play(CustomCastSfx);
        await CreatureCmd.TriggerAnim(Creature, "Cast", 0.75f);
    }

    private static bool IsLivingBishop(Creature creature) {
        return creature.IsAlive && creature.Monster is IvoryBishop or EbonyBishop;
    }

    public override CreatureAnimator SetupCustomAnimationStates(MegaSprite controller) {
        AnimState idle = new("idle_loop", isLooping: true);
        AnimState attack = new("attack");
        AnimState cast = new("cast");
        AnimState hurt = new("hurt");
        AnimState die = new("die");
        attack.NextState = idle;
        cast.NextState = idle;
        hurt.NextState = idle;

        CreatureAnimator animator = new(idle, controller);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Cast", cast);
        animator.AddAnyState("Dead", die);
        animator.AddAnyState("Hit", hurt);
        return animator;
    }
}
