using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Powers;

public class StrengthDecayPower : CustomPowerModel {
    private const string PowerIconPath = "res://TheGleaner/images/powers/demomod-strength_decay_power.png";
    public override string CustomPackedIconPath => PowerIconPath;
    public override string CustomBigIconPath => PowerIconPath;
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side) {
        _ = choiceContext;
        if (side != Owner.Side) {
            return;
        }

        Flash();
        int amount = Amount;
        await PowerCmd.Remove(this);
        await PowerCmd.Apply<StrengthPower>(Owner, -amount, Owner, null);
    }
}
