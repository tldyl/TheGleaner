using BaseLib.Abstracts;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace DemoMod.TheGleaner.Monsters;

public class Inkcrow : CustomMonsterModel {
    public override int MinInitialHp => 22;
    public override int MaxInitialHp => 28;
    
    public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/owl_magistrate");
    
    private int PeckCount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private int BashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 13);
    private int LandPeckDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);
    private int SlipperyAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

    public int slotIndex = 0;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        List<MonsterState> states = [];
        ConditionalBranchState initialState = new ConditionalBranchState("INITIAL_BRANCH");
        MoveState peckState = new MoveState("PECK_MOVE", PeckMove, new MultiAttackIntent(1, PeckCount));
        MoveState growState = new MoveState("GROWN_MOVE", GrowMove, new BuffIntent());
        MoveState bashState = new MoveState("BASH_MOVE", BashMove, new SingleAttackIntent(BashDamage));
        peckState.FollowUpState = growState;
        growState.FollowUpState = bashState;
        bashState.FollowUpState = peckState;
        initialState.AddState(peckState, () => slotIndex is 0 or 3);
        initialState.AddState(growState, () => slotIndex == 1);
        initialState.AddState(bashState, () => slotIndex == 2);

        MoveState landPeckState = new MoveState("LAND_PECK_MOVE", LandPeckMove, new SingleAttackIntent(LandPeckDamage));
        MoveState regainSlipperyState = new MoveState("REGAIN_SLIPPERY_MOVE", RegainSlipperyMove, new BuffIntent());
        landPeckState.FollowUpState = regainSlipperyState;
        regainSlipperyState.FollowUpState = peckState;
        
        states.Add(initialState);
        states.Add(peckState);
        states.Add(growState);
        states.Add(bashState);
        states.Add(landPeckState);
        states.Add(regainSlipperyState);
        return new MonsterMoveStateMachine(states, initialState);
    }

    public override async Task AfterAddedToRoom() {
        await PowerCmd.Apply<SlipperyPower>(new ThrowingPlayerChoiceContext(), Creature, SlipperyAmount, Creature, null);
        await PowerCmd.Apply<InkwingPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        await CreatureCmd.TriggerAnim(Creature, "TakeOff", 0.0f);
    }
    
    private async Task PeckMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(1)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.5f)
            //.WithAttackerFx(null, CustomAttackSfx)
            .WithHitCount(PeckCount)
            .Execute(null);
    }

    private async Task GrowMove(IReadOnlyList<Creature> targets) {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    private async Task BashMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(BashDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.5f)
            //.WithAttackerFx(null, CustomAttackSfx)
            .Execute(null);
    }

    private async Task LandPeckMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(LandPeckDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.5f)
            //.WithAttackerFx(null, CustomAttackSfx)
            .Execute(null);
    }

    private async Task RegainSlipperyMove(IReadOnlyList<Creature> targets) {
        await PowerCmd.Apply<SlipperyPower>(new ThrowingPlayerChoiceContext(), Creature, SlipperyAmount, Creature, null);
        await PowerCmd.Apply<InkwingPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        await CreatureCmd.TriggerAnim(Creature, "TakeOff", 0.0f);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller) {
        AnimState initialState = new AnimState("idle_loop", true);
        AnimState state1 = new AnimState("attack_peck");
        AnimState state2 = new AnimState("hurt");
        AnimState state3 = new AnimState("die");
        AnimState state4 = new AnimState("take_off");
        AnimState animState = new AnimState("fly_loop", true)
        {
            BoundsContainer = "FlyingBounds"
        };
        AnimState state5 = new AnimState("attack_dive")
        {
            BoundsContainer = "IdleBounds"
        };
        AnimState state6 = new AnimState("hurt_flying");
        AnimState state7 = new AnimState("die_flying");
        initialState.AddBranch("Attack", state1);
        initialState.AddBranch("Hit", state2);
        initialState.AddBranch("Dead", state3);
        state1.NextState = initialState;
        state1.AddBranch("Hit", state2);
        state1.AddBranch("Dead", state3);
        state2.NextState = initialState;
        state2.AddBranch("Attack", state1);
        state2.AddBranch("Dead", state3);
        state2.AddBranch("Hit", state2);
        state4.NextState = animState;
        state4.AddBranch("Hit", state6);
        state4.AddBranch("Dead", state7);
        animState.AddBranch("Attack", state5);
        animState.AddBranch("Hit", state6);
        animState.AddBranch("Dead", state7);
        state6.NextState = animState;
        state6.AddBranch("Attack", state5);
        state6.AddBranch("Dead", state7);
        state5.NextState = animState;
        state5.AddBranch("Attack", state5);
        state5.AddBranch("Dead", state3);
        CreatureAnimator animator = new CreatureAnimator(initialState, controller);
        animator.AddAnyState("TakeOff", state4);
        animator.AddAnyState("IdleLoop", initialState);
        return animator;
    }
}
