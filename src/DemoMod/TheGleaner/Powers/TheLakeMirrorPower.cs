using BaseLib.Abstracts;
using DemoMod.TheGleaner.Enums;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace DemoMod.TheGleaner.Powers;

public class TheLakeMirrorPower : CustomPowerModel {
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
        if (cardPlay.Card.Owner != Owner.Player) {
            return;
        }

        if (cardPlay.Card.Keywords.Contains(CustomEnums.Resonance)) {
            Flash();
            await PowerCmd.Apply<DemoTempStrengthPower>(
                Owner,
                Amount,
                Owner,
                null
            );
            await PowerCmd.Apply<DemoTempDexterityPower>(
                Owner,
                Amount,
                Owner,
                null
            );
        }
    }
}
