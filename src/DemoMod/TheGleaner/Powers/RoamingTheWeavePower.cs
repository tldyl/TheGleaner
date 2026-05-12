using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace DemoMod.TheGleaner.Powers;

public class RoamingTheWeavePower : CustomPowerModel {
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Decimal ModifyEnergyGain(Player player, Decimal amount) {
        if (player != Owner.Player) {
            return amount;
        }
        return amount + Amount;
    }
}
