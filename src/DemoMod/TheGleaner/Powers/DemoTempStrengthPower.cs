using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace DemoMod.TheGleaner.Powers;

public class DemoTempStrengthPower : CustomPowerModel {
    private bool _shouldIgnoreNextInstance;
    public override string CustomPackedIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string CustomBigIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => true;

    public override LocString Title => new LocString("powers", "DEMOMOD-TEMP_STRENGTH_POWER.title");

    public override LocString Description => new LocString("powers", "DEMOMOD-TEMP_STRENGTH_POWER.description");

    protected override string SmartDescriptionLocKey =>
        "DEMOMOD-TEMP_STRENGTH_POWER.smartDescription";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public void IgnoreNextInstance() {
        _shouldIgnoreNextInstance = true;
    }

    public override async Task BeforeApplied(
        Creature target,
        decimal amount,
        Creature applier,
        CardModel cardSource) {
        if (_shouldIgnoreNextInstance) {
            _shouldIgnoreNextInstance = false;
        } else {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), target, amount, applier, cardSource, true);
        }
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        Decimal amount,
        Creature? applier,
        CardModel? cardSource) {
        if (amount == Amount) {
            return;
        }

        if (power != this) {
            return;
        }

        if (_shouldIgnoreNextInstance) {
            _shouldIgnoreNextInstance = false;
        } else {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, amount, applier, cardSource, true);
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> _) {
        if (side == Owner.Side) {
            Flash();
            await PowerCmd.Remove(this);
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, -Amount, Owner, null, false);
        }
    }
}
