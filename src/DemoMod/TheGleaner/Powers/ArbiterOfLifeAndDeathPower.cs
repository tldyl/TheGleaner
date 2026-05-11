using BaseLib.Abstracts;
using DemoMod.TheGleaner.Afflictions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using DemoMod.TheGleaner.Monsters;

namespace DemoMod.TheGleaner.Powers;

public class ArbiterOfLifeAndDeathPower : CustomPowerModel {
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override object InitInternalData() => new Data();
    private bool IsReviving => GetInternalData<Data>().isReviving;

    public void DoRevive() => GetInternalData<Data>().isReviving = false;

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength) {
        if (wasRemovalPrevented || creature != Owner || creature.Monster is not KnightOfWarsEnd monster)
            return;
        GetInternalData<Data>().isReviving = true;
        await monster.TriggerDeadState();
    }
    
    public override bool ShouldAllowHitting(Creature creature) {
        return creature != Owner || !IsReviving;
    }
    
    public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature) {
        return creature != Owner;
    }
    public override bool ShouldStopCombatFromEnding() => true;
    public override bool ShouldPowerBeRemovedAfterOwnerDeath() => false;

    public override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw) {
        AfflictionModel affliction = GetInternalData<Data>().NextAffliction();
        CardCmd.ClearAffliction(card);
        await CardCmd.Afflict(affliction, card, 1);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
        CardModel card = cardPlay.Card;
        AfflictionModel affliction = card.Affliction;
        if (affliction is FlameOfDeath) {
            await CreatureCmd.Damage(context, card.Owner.Creature, new DamageVar(2, ValueProp.Move | ValueProp.Unpowered | ValueProp.Unblockable), card);
            await PowerCmd.Apply<DemoTempStrengthPower>(card.Owner.Creature, -1, card.Owner.Creature, card);
        } else if (affliction is LightOfLife) {
            await CreatureCmd.Heal(Owner, 5);
            await PowerCmd.Apply<DemoTempStrengthPower>(Owner, 1, card.Owner.Creature, card);
        }
    }
    
    private class Data {
        public bool isReviving;

        private readonly List<AfflictionModel> pool = [];

        public AfflictionModel NextAffliction() {
            if (pool.Count == 0) {
                initPool();
            }
            AfflictionModel ret = pool[0];
            pool.RemoveAt(0);
            return ret;
        }

        private void initPool() {
            pool.Add(ModelDb.Affliction<LightOfLife>().ToMutable());
            pool.Add(ModelDb.Affliction<FlameOfDeath>().ToMutable());
            pool.StableShuffle(RunManager.Instance.DebugOnlyGetState().Rng.CombatCardGeneration);
        }
    }
}
