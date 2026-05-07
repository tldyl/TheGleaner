using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace DemoMod.TheGleaner.Powers;

public class PromotionPower : CustomPowerModel {
    private const string PowerIconPath = "res://TheGleaner/images/powers/demomod-staff_burnout_power.png";
    public override string CustomPackedIconPath => PowerIconPath;
    public override string CustomBigIconPath => PowerIconPath;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public void TriggerFlash() {
        Flash();
    }
}
