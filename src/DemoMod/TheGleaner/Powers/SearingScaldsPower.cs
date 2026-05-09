using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Powers;

public class SearingScaldsPower : CustomPowerModel {
    private const string PowerIconPath = "res://TheGleaner/images/powers/demomod-searing_scalds_power.png";

    public override string CustomPackedIconPath => PowerIconPath;
    public override string CustomBigIconPath => PowerIconPath;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Burn>()
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
        if (!target.IsPlayer || result.UnblockedDamage <= 0) {
            return;
        }

        Flash();
        await CardPileCmd.AddToCombatAndPreview<Burn>(
            target,
            PileType.Discard,
            1,
            addedByPlayer: false,
            CardPilePosition.Random);
    }
}
