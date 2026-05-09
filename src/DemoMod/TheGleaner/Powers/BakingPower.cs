using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Powers;

public class BakingPower : CustomPowerModel {
    private const string PowerIconPath = "res://TheGleaner/images/powers/demomod-baking_power.png";
    private const string BonusDamageKey = "BonusDamage";

    public override string CustomPackedIconPath => PowerIconPath;
    public override string CustomBigIconPath => PowerIconPath;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar(BonusDamageKey, 0)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Burn>()
    ];

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource) {
        _ = target;
        _ = amount;
        _ = props;
        _ = dealer;
        return cardSource is Burn ? DynamicVars[BonusDamageKey].BaseValue : 0m;
    }

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource) {
        _ = choiceContext;
        _ = props;
        _ = dealer;
        if (!target.IsPlayer || !result.WasFullyBlocked || cardSource is not Burn) {
            return Task.CompletedTask;
        }

        Flash();
        DynamicVars[BonusDamageKey].BaseValue += 1m;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }
}
