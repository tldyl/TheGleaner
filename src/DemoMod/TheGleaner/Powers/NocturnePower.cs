using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Entities.Powers;
namespace DemoMod.TheGleaner.Powers;

public class NocturnePower : CustomPowerModel {
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side) {
        if (side == Owner.Side || Owner.Block < 10) {
            return;
        }

        Flash();

        await PowerCmd.Apply<StrengthPower>(Owner, Amount - 1, Owner, null);
        await PowerCmd.Apply<DemoTempDexterityPower>(Owner, Amount, Owner, null);
    }
}