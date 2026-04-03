using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using DemoMod.TheGleaner.Enums;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace DemoMod.TheGleaner.Powers;

public class ReverberationPower : CustomPowerModel {
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side) {
        if (side != Owner.Side) {
            return;
        }

        if (Owner.Player?.PlayerCombatState == null) {
            return;
        }

        CardPile? scorePile = CustomPiles.GetCustomPile(Owner.Player.PlayerCombatState, CustomEnums.ScorePile);
        int count = scorePile?.Cards.Count ?? 0;
        if (count <= 0) {
            return;
        }

        Flash();

        await CreatureCmd.GainBlock(
            Owner,
            count * Amount,
            ValueProp.Unpowered,
            null
        );
    }
}