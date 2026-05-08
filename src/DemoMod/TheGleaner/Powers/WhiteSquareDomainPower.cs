using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Powers;

public class WhiteSquareDomainPower : CustomPowerModel {
    private const string PowerIconPath = "res://TheGleaner/images/powers/demomod-white_square_domain_power.png";
    public override string CustomPackedIconPath => PowerIconPath;
    public override string CustomBigIconPath => PowerIconPath;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
        if (cardPlay.Card.Owner.Creature?.IsPlayer != true || cardPlay.Card.Type != CardType.Skill) {
            return;
        }

        Flash();
        await PowerCmd.Apply<StrengthPower>(Owner, Amount, Owner, null);
        await PowerCmd.Apply<StrengthDecayPower>(Owner, Amount, Owner, null);
    }
}
