using BaseLib.Abstracts;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace DemoMod.TheGleaner.Monsters;

public class ZapfliSorceress : CustomMonsterModel {
    public override int MinInitialHp => 120;
    public override int MaxInitialHp => 130;
    
    public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/fabricator");
    
    private int LightBluntDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 6);
    private int HeavyBluntDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 12);

    private bool CanSummon2 {
        get {
            return Creature.CombatState.GetTeammatesOf(Creature).Count(c => c.IsAlive) <= 3;
        }
    }
    
    private bool CanSummon1 {
        get {
            return Creature.CombatState.GetTeammatesOf(Creature).Count(c => c.IsAlive) <= 4;
        }
    }
    
    private bool CanBuff {
        get {
            return Creature.CombatState.GetTeammatesOf(Creature).Count(c => c.IsAlive) >= 3;
        }
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        List<MonsterState> states = [];
        MoveState initialState = new MoveState("LIGHT_BLUNT_MOVE", LightBluntMove, new SingleAttackIntent(LightBluntDamage), new SummonIntent());
        MoveState heavyBluntState = new MoveState("HEAVY_BLUNT_MOVE", HeavyBluntMove, new SingleAttackIntent(HeavyBluntDamage), new SummonIntent());
        MoveState heavyBluntBuffState = new MoveState("HEAVY_BLUNT_BUFF_MOVE", HeavyBluntBuffMove, new SingleAttackIntent(HeavyBluntDamage), new BuffIntent());

        ConditionalBranchState conditionalBranchState = new ConditionalBranchState("COND_MOVE");
        conditionalBranchState.AddState(initialState, () => CanSummon2);
        conditionalBranchState.AddState(heavyBluntState, () => CanSummon1);
        conditionalBranchState.AddState(heavyBluntBuffState, () => CanBuff);

        initialState.FollowUpState = conditionalBranchState;
        heavyBluntState.FollowUpState = conditionalBranchState;
        heavyBluntBuffState.FollowUpState = conditionalBranchState;
        
        states.Add(initialState);
        states.Add(heavyBluntState);
        states.Add(heavyBluntBuffState);
        states.Add(conditionalBranchState);
        
        return new MonsterMoveStateMachine(states, initialState);
    }

    private async Task SummonZapfli(int amount) {
        for (int _ = 0; _ < amount; _++) {
            string slotName = CombatState.Encounter.Slots.LastOrDefault(s => CombatState.Enemies.All(c => c.SlotName != s), string.Empty);
            await CreatureCmd.Add<Zapfli>(CombatState, slotName);
        }
    }
    
    private async Task LightBluntMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(LightBluntDamage)
            .FromMonster(this)
            .Execute(null);
        await SummonZapfli(2);
    }
    
    private async Task HeavyBluntMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(HeavyBluntDamage)
            .FromMonster(this)
            .Execute(null);
        await SummonZapfli(1);
    }
    
    private async Task HeavyBluntBuffMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(HeavyBluntDamage)
            .FromMonster(this)
            .Execute(null);
        foreach (Creature creature in Creature.CombatState.GetTeammatesOf(Creature)) {
            await PowerCmd.Apply<StrengthPower>(creature, 3, Creature, null);
        }
    }
}
