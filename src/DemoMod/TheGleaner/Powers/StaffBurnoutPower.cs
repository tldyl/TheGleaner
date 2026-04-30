using BaseLib.Abstracts;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Powers;

public class StaffBurnoutPower : CustomPowerModel {
    public override string CustomPackedIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string CustomBigIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardChangedPiles(
        CardModel card,
        PileType oldPileType,
        AbstractModel? source) {
        if (oldPileType == CustomEnums.ScorePile) {
            Flash();
            IEnumerable<DamageResult> _ = await CreatureCmd.Damage(
                new ThrowingPlayerChoiceContext(),
                CombatState.HittableEnemies,
                ScorePileCmd.GetCapacity(Owner.Player) * Amount,
                ValueProp.Unpowered,
                Owner,
                null
            );
        }
    }
    
    public override async Task AfterBlockCleared(Creature creature) {
        if (creature != Owner) {
            return;
        }
        await PowerCmd.Remove(this);
    }
}
