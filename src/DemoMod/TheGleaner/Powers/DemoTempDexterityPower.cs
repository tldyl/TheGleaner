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

public class DemoTempDexterityPower : CustomPowerModel
{
    private bool _shouldIgnoreNextInstance;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => true;

    public override LocString Title =>
        new("powers", "DEMOMOD-TEMP_DEXTERITY_POWER.title");

    public override LocString Description =>
        new("powers", "DEMOMOD-TEMP_DEXTERITY_POWER.description");

    protected override string SmartDescriptionLocKey =>
        "DEMOMOD-TEMP_DEXTERITY_POWER.smartDescription";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DexterityPower>()
    ];

    public void IgnoreNextInstance()
    {
        _shouldIgnoreNextInstance = true;
    }

    public override async Task BeforeApplied(
        Creature target,
        decimal amount,
        Creature applier,
        CardModel cardSource)
    {
        if (_shouldIgnoreNextInstance)
        {
            _shouldIgnoreNextInstance = false;
        }
        else
        {
            await PowerCmd.Apply<DexterityPower>(target, amount, applier, cardSource, true);
        }
    }

    public override async Task AfterPowerAmountChanged(
        PowerModel power,
        decimal amount,
        Creature applier,
        CardModel cardSource)
    {
        if (amount == Amount)
        {
            return;
        }

        if (power != this)
        {
            return;
        }

        if (_shouldIgnoreNextInstance)
        {
            _shouldIgnoreNextInstance = false;
        }
        else
        {
            await PowerCmd.Apply<DexterityPower>(Owner, amount, applier, cardSource, true);
        }
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == Owner.Side)
        {
            Flash();
            await PowerCmd.Remove(this);
            await PowerCmd.Apply<DexterityPower>(Owner, -Amount, Owner, null, false);
        }
    }
}