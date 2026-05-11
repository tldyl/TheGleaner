using BaseLib.Abstracts;
using DemoMod.TheGleaner.Afflictions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Powers;

public class ClearDeclarationOfTheEndPower : CustomPowerModel {
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar("DisplayAmount", 0)
    ];
    public override int DisplayAmount => DynamicVars["DisplayAmount"].IntValue;
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
        AfflictionModel affliction = cardPlay.Card.Affliction;
        if (affliction is FlameOfDeath) {
            Flash();
            DynamicVars["DisplayAmount"].BaseValue++;
            InvokeDisplayAmountChanged();
            if (DynamicVars["DisplayAmount"].BaseValue >= 12) {
                foreach (Creature creature in Owner.CombatState.Enemies) {
                    if (creature.HasPower<DeclarationOfTheEndPower>()) {
                        DeclarationOfTheEndPower power = creature.GetPower<DeclarationOfTheEndPower>();
                        power.DynamicVars["DisplayAmount"].BaseValue = 0;
                        power.RefreshCounter();
                    }
                }
                DynamicVars["DisplayAmount"].BaseValue = 0;
                InvokeDisplayAmountChanged();
            }
        }
    }
}
