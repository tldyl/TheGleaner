using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DemoMod.TheGleaner.Powers;

public class DeclarationOfTheEndPower : CustomPowerModel {
    public override string CustomPackedIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string CustomBigIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar("DisplayAmount", -1)
    ];

    public override int DisplayAmount => DynamicVars["DisplayAmount"].IntValue;

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side) {
        if (side == Owner.Side) {
            Flash();
            DynamicVars["DisplayAmount"].BaseValue++;
            InvokeDisplayAmountChanged();
        }
    }

    public void RefreshCounter() {
        InvokeDisplayAmountChanged();
    }
}
