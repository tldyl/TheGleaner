using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace DemoMod.TheGleaner.Powers;

public class SuperpositionPrepPower : CustomPowerModel {
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState) {
        if (side != Owner.Side) {
            return;
        }

        await PowerCmd.Apply<SuperpositionReadyPower>(Owner, Amount, Owner, null, false);
        await PowerCmd.Remove(this);
    }
}