using BaseLib.Abstracts;
using DemoMod.TheGleaner.Monsters;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Powers;

public class StartledDreamPower : CustomPowerModel {
    private const string PowerIconPath = "res://TheGleaner/images/powers/demomod-startled_dream_power.png";

    public override string CustomPackedIconPath => PowerIconPath;
    public override string CustomBigIconPath => PowerIconPath;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<SoarPower>(),
        HoverTipFactory.FromPower<PlatingPower>()
    ];

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource) {
        _ = choiceContext;
        _ = props;
        _ = dealer;
        _ = cardSource;
        if (target != Owner || result.UnblockedDamage <= 0 || Owner.Monster is not SleepyScarecrow scarecrow) {
            return;
        }

        Flash();
        await scarecrow.WakeUp(startleCrows: true);
        await PowerCmd.Remove(this);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side) {
        _ = choiceContext;
        if (side != Owner.Side || Owner.Monster is not SleepyScarecrow scarecrow) {
            return;
        }

        if (Amount > 1) {
            await PowerCmd.Decrement(this);
            return;
        }

        Flash();
        await scarecrow.WakeUp(startleCrows: false);
        await PowerCmd.Remove(this);
    }
}
