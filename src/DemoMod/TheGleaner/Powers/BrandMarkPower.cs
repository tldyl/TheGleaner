using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Powers;

public class BrandMarkPower : CustomPowerModel {
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private Creature target;
    private Creature? dealer;
    private ValueProp props;
    
    public override Decimal ModifyHpLostAfterOstyLate(
        Creature target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource) {
        this.target = target;
        this.dealer = dealer;
        this.props = props;
        return amount;
    }
    
    public override async Task AfterModifyingHpLostAfterOsty() {
        if (target == Owner && !props.HasFlag(ValueProp.Unpowered) && dealer != null) {
            Flash();
            await CreatureCmd.GainBlock(dealer, 3, ValueProp.Unpowered, null);
            await PowerCmd.Decrement(this);
        }
    }
}
