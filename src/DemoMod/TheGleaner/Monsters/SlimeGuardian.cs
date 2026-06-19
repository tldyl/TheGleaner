using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Monsters;

public class SlimeGuardian : CustomMonsterModel {
    public override int MinInitialHp => 20;
    public override int MaxInitialHp => 20;
    private int DashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);
    private int BashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 15);

    public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/twig_slime_m");
    
    private bool CanSummon {
        get {
            return Creature.CombatState.GetTeammatesOf(Creature).Count(c => c.IsAlive) <= 3;
        }
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        List<MonsterState> states = [];
        MoveState initialState = new MoveState("DEFEND_MOVE", DefendMove, new DefendIntent(), new SummonIntent());
        MoveState frailState = new MoveState("FRAIL_MOVE", FrailMove, new DebuffIntent(), new SummonIntent());
        initialState.FollowUpState = frailState;
        ConditionalBranchState dashBranchState = new ConditionalBranchState("DASH_BRANCH");
        frailState.FollowUpState = dashBranchState;
        MoveState dashSummonState = new MoveState("DASH_SUMMON_MOVE", DashSummonMove, new SingleAttackIntent(DashDamage), new DefendIntent(), new SummonIntent());
        MoveState dashStatusState = new MoveState("DASH_STATUS_MOVE", DashStatusMove, new SingleAttackIntent(DashDamage), new DefendIntent(), new StatusIntent(2));
        dashBranchState.AddState(dashSummonState, () => CanSummon);
        dashBranchState.AddState(dashStatusState, () => !CanSummon);
        MoveState bashSummonState = new MoveState("BASH_SUMMON_MOVE", BashSummonMove, new SingleAttackIntent(BashDamage), new SummonIntent());
        MoveState bashStatusState = new MoveState("BASH_STATUS_MOVE", BashStatusMove, new SingleAttackIntent(BashDamage), new StatusIntent(2));
        ConditionalBranchState bashBranchState = new ConditionalBranchState("BASH_BRANCH");
        bashBranchState.AddState(bashSummonState, () => CanSummon);
        bashBranchState.AddState(bashStatusState, () => !CanSummon);
        dashSummonState.FollowUpState = bashBranchState;
        dashStatusState.FollowUpState = bashBranchState;
        bashSummonState.FollowUpState = dashBranchState;
        bashStatusState.FollowUpState = dashBranchState;
        states.Add(initialState);
        states.Add(frailState);
        states.Add(dashSummonState);
        states.Add(dashStatusState);
        states.Add(bashSummonState);
        states.Add(bashStatusState);
        states.Add(dashBranchState);
        states.Add(bashBranchState);
        return new MonsterMoveStateMachine(states, initialState);
    }

    public override async Task AfterAddedToRoom() {
        await PowerCmd.Apply<BarricadePower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player) {
        if (CombatState.RoundNumber == 1) {
            await CreatureCmd.GainBlock(Creature, 15, ValueProp.Unpowered, null);
        }
    }
    
    private async Task SummonRandomSlime() {
        string slotName = CombatState.Encounter.Slots.LastOrDefault(s => CombatState.Enemies.All(c => c.SlotName != s), string.Empty);
        if (Rng.NextBool()) {
            await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), await CreatureCmd.Add<TwigSlimeS>(CombatState, slotName), 1m, Creature, null);
        } else {
            await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), await CreatureCmd.Add<LeafSlimeS>(CombatState, slotName), 1m, Creature, null);
        }
    }
    
    private async Task GoopMove(IReadOnlyList<Creature> targets) {
        await CreatureCmd.TriggerAnim(Creature, "Cast", 0.5f);
        SfxCmd.Play(AttackSfx);
        VfxCmd.PlayOnCreatureCenters(targets, "vfx/vfx_slime_impact");
        await CardPileCmd.AddToCombatAndPreview<Slimed>(targets, PileType.Discard, 2, null);
    }
    
    private async Task DefendMove(IReadOnlyList<Creature> targets) {
        await CreatureCmd.GainBlock(Creature, 15, ValueProp.Move, null);
        await SummonRandomSlime();
    }
    
    private async Task FrailMove(IReadOnlyList<Creature> targets) {
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, 3, Creature, null);
        await SummonRandomSlime();
    }
    
    private async Task DashSummonMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(DashDamage)
            .FromMonster(this)
            //.WithAttackerAnim("Attack", 0.15f)
            //.WithAttackerFx(null, CustomAttackSfx)
            .WithHitFx("vfx/vfx_slime_impact")
            .Execute(null);
        await CreatureCmd.GainBlock(Creature, 10, ValueProp.Move, null);
        await SummonRandomSlime();
    }
    
    private async Task BashSummonMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(BashDamage)
            .FromMonster(this)
            //.WithAttackerAnim("Attack", 0.15f)
            //.WithAttackerFx(null, CustomAttackSfx)
            .WithHitFx("vfx/vfx_slime_impact")
            .Execute(null);
        await SummonRandomSlime();
    }
    
    private async Task DashStatusMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(DashDamage)
            .FromMonster(this)
            //.WithAttackerAnim("Attack", 0.15f)
            //.WithAttackerFx(null, CustomAttackSfx)
            .WithHitFx("vfx/vfx_slime_impact")
            .Execute(null);
        await CreatureCmd.GainBlock(Creature, 10, ValueProp.Move, null);
        await GoopMove(targets);
    }
    
    private async Task BashStatusMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(BashDamage)
            .FromMonster(this)
            //.WithAttackerAnim("Attack", 0.15f)
            //.WithAttackerFx(null, CustomAttackSfx)
            .WithHitFx("vfx/vfx_slime_impact")
            .Execute(null);
        await GoopMove(targets);
    }
}
