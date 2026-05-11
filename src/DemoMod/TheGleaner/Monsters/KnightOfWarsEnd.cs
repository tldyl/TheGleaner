using BaseLib.Abstracts;
using DemoMod.TheGleaner.Afflictions;
using DemoMod.TheGleaner.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace DemoMod.TheGleaner.Monsters;

public class KnightOfWarsEnd : CustomMonsterModel {
    public override int MinInitialHp => 270;
    public override int MaxInitialHp => 270;
    public override bool HasDeathSfx => false;
    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;
    public override string CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/living_shield");
    private int DeathFlameCycleDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);
    private int BuffedDeathFlameCycleDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 15);
    private int ClawDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 22, 20);
    private int BuffedClawDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 32, 30);
    private MoveState _deadState;
    private MoveState DeadState {
        get => _deadState;
        set {
            AssertMutable();
            _deadState = value;
        }
    }
    private int _respawns;
    private int Respawns {
        get => _respawns;
        set {
            AssertMutable();
            _respawns = value;
        }
    }
    private int _extraMultiClawCount;
    private int ExtraMultiClawCount {
        get => _extraMultiClawCount;
        set {
            AssertMutable();
            _extraMultiClawCount = value;
        }
    }
    public override bool ShouldDisappearFromDoom => Respawns >= 1;
    private int MultiClawTotalCount => 2 + ExtraMultiClawCount;

    private string lastMoveId;
    
    public async Task TriggerDeadState() {
        SetMoveImmediate(DeadState, true);
    }
    
    public override async Task AfterAddedToRoom() {
        await CreatureCmd.SetMaxAndCurrentHp(Creature, 999999999M);
        Creature.ShowsInfiniteHp = true;
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine() {
        List<MonsterState> states = [];
        DeadState = new MoveState("RESPAWN_MOVE", RespawnMove, new HealIntent(), new BuffIntent()) {
            MustPerformOnceBeforeTransitioning = true
        };
        MoveState initialState = new MoveState("ARBITER_OF_LIFE_AND_DEATH_MOVE", ArbiterOfLifeAndDeathMove, new SummonIntent());
        MoveState deathFlameCycle = new MoveState("DEATH_FLAME_CYCLE_MOVE", DeathFlameCycleMove, new MultiAttackIntent(DeathFlameCycleDamage, () => MultiClawTotalCount));
        MoveState clawState = new MoveState("CLAW_MOVE", ClawMove, new SingleAttackIntent(ClawDamage), new DebuffIntent());
        MoveState buffedDeathFlameCycle = new MoveState("BUFFED_DEATH_FLAME_CYCLE_MOVE", BuffedDeathFlameCycleMove, new MultiAttackIntent(BuffedDeathFlameCycleDamage, () => MultiClawTotalCount));
        MoveState buffedClawState = new MoveState("BUFFED_CLAW_MOVE", BuffedClawMove, new SingleAttackIntent(BuffedClawDamage), new BuffIntent());
        MoveState declarationOfTheEndState = new MoveState("DECLARATION_OF_THE_END_MOVE", DeclarationOfTheEndMove, new MultiAttackIntent(DeathFlameCycleDamage, 7));
        initialState.FollowUpState = deathFlameCycle;
        deathFlameCycle.FollowUpState = clawState;
        clawState.FollowUpState = deathFlameCycle;
        DeadState.FollowUpState = buffedDeathFlameCycle;
        ConditionalBranchState conditionalBranchState = new ConditionalBranchState("ENDING_BRANCH");
        buffedDeathFlameCycle.FollowUpState = conditionalBranchState;
        buffedClawState.FollowUpState = conditionalBranchState;
        declarationOfTheEndState.FollowUpState = conditionalBranchState;
        conditionalBranchState.AddState(buffedClawState, () => Creature.GetPower<DeclarationOfTheEndPower>().DynamicVars["DisplayAmount"].IntValue < 3 && "BUFFED_DEATH_FLAME_CYCLE_MOVE".Equals(lastMoveId));
        conditionalBranchState.AddState(buffedDeathFlameCycle, () => Creature.GetPower<DeclarationOfTheEndPower>().DynamicVars["DisplayAmount"].IntValue < 3 && "BUFFED_CLAW_MOVE".Equals(lastMoveId));
        conditionalBranchState.AddState(declarationOfTheEndState, () => Creature.GetPower<DeclarationOfTheEndPower>().DynamicVars["DisplayAmount"].IntValue >= 3);
        states.Add(DeadState);
        states.Add(initialState);
        states.Add(deathFlameCycle);
        states.Add(clawState);
        states.Add(buffedClawState);
        states.Add(buffedDeathFlameCycle);
        states.Add(declarationOfTheEndState);
        states.Add(conditionalBranchState);
        return new MonsterMoveStateMachine(states, initialState);
    }

    private async Task ArbiterOfLifeAndDeathMove(IReadOnlyList<Creature> targets) {
        await CreatureCmd.SetMaxAndCurrentHp(Creature, 270);
        foreach (PowerModel power in Creature.Powers.ToList())
            await PowerCmd.Remove(power);
        Creature.ShowsInfiniteHp = false;
        await PowerCmd.Apply<ArbiterOfLifeAndDeathPower>(Creature, 1, Creature, null);
    }

    private async Task DeathFlameCycleMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(DeathFlameCycleDamage)
            .WithHitCount(MultiClawTotalCount)
            .FromMonster(this)
            .OnlyPlayAnimOnce()
            .WithNoAttackerAnim()
            .Execute(null);
        ExtraMultiClawCount++;
    }

    private async Task ClawMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(ClawDamage)
            .FromMonster(this)
            .WithNoAttackerAnim()
            .Execute(null);
        await PowerCmd.Apply<VulnerablePower>(targets, 1, Creature, null);
    }

    private async Task BuffedDeathFlameCycleMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(BuffedDeathFlameCycleDamage)
            .WithHitCount(MultiClawTotalCount)
            .FromMonster(this)
            .OnlyPlayAnimOnce()
            .WithNoAttackerAnim()
            .Execute(null);
        ExtraMultiClawCount++;
        lastMoveId = "BUFFED_DEATH_FLAME_CYCLE_MOVE";
    }

    private async Task BuffedClawMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(BuffedClawDamage)
            .FromMonster(this)
            .WithNoAttackerAnim()
            .Execute(null);
        await PowerCmd.Apply<StrengthPower>(Creature, 2, Creature, null);
        lastMoveId = "BUFFED_CLAW_MOVE";
    }
    
    private async Task RespawnMove(IReadOnlyList<Creature> targets) {
        Respawns++;
        Creature.GetPower<ArbiterOfLifeAndDeathPower>()?.DoRevive();
        
        ExtraMultiClawCount = 0;
        List<CardModel> _allCards = (List<CardModel>) AccessTools.Field(typeof(CombatState), "_allCards").GetValue(Creature.CombatState);
        foreach (CardModel card in _allCards) {
            if (card.Affliction is LightOfLife or FlameOfDeath) {
                card.Affliction.Amount = 2;
            }
        }
        await Revive(300);
        await PowerCmd.Remove<ArbiterOfLifeAndDeathPower>(Creature);
        await PowerCmd.Apply<DeclarationOfTheEndPower>(Creature, 1, Creature, null);
        await PowerCmd.Apply<ClearDeclarationOfTheEndPower>(targets, 1, Creature, null);
        await PowerCmd.Apply<LightOfLifePower>(targets, 1, Creature, null);
    }

    private async Task Revive(int baseRespawnHp) {
        AssertMutable();
        int scaledHp = baseRespawnHp * Creature.CombatState.Players.Count;
        await CreatureCmd.SetMaxHp(Creature, scaledHp);
        await CreatureCmd.Heal(Creature, scaledHp);
    }

    private async Task DeclarationOfTheEndMove(IReadOnlyList<Creature> targets) {
        await DamageCmd.Attack(DeathFlameCycleDamage)
            .WithHitCount(7)
            .FromMonster(this)
            .OnlyPlayAnimOnce()
            .WithNoAttackerAnim()
            .Execute(null);
    }
}
