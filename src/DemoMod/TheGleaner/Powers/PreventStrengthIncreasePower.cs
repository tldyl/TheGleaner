using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Powers;

public class PreventStrengthIncreasePower : CustomPowerModel {
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;
    
    public override bool TryModifyPowerAmountReceived(
        PowerModel canonicalPower,
        Creature target,
        Decimal amount,
        Creature? _,
        out Decimal modifiedAmount) {
        if (target != Owner) {
            modifiedAmount = amount;
            return false;
        }
        if (canonicalPower.GetTypeForAmount(amount) != PowerType.Buff) {
            modifiedAmount = amount;
            return false;
        }
        if (!canonicalPower.IsVisible) {
            modifiedAmount = amount;
            return false;
        }
        if (canonicalPower is not StrengthPower) {
            modifiedAmount = amount;
            return false;
        }
        modifiedAmount = 0M;
        return true;
    }

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState) {
        if (side == Owner.Side) {
            return;
        }
        await PowerCmd.Remove(this);
    }
}
