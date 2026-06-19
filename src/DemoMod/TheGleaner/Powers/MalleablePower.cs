using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Powers;

public class MalleablePower : CustomPowerModel {
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => DynamicVars["DisplayAmount"].IntValue;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar("DisplayAmount", 3)
    ];

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource) {
        DynamicVars["DisplayAmount"].BaseValue = Amount;
        InvokeDisplayAmountChanged();
    }
    
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> _) {
        if (side != Owner.Side) {
            DynamicVars["DisplayAmount"].BaseValue = Amount;
            InvokeDisplayAmountChanged();
        }
    }
    
    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource) {
        if (target != Owner || result.UnblockedDamage == 0 || props.HasFlag(ValueProp.Unpowered)) {
            return;
        }
        Flash();
        await CreatureCmd.GainBlock(Owner, DynamicVars["DisplayAmount"].BaseValue, ValueProp.Unpowered, null, true);
        DynamicVars["DisplayAmount"].BaseValue++;
        InvokeDisplayAmountChanged();
    }
}
