using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Powers;

public class StaffSurgingPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public decimal ModifyVulnerableMultiplier(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature dealer,
        CardModel? cardSource)
    {
        if (target != Owner)
        {
            return amount;
        }

        return amount + (amount - 1m);
    }

    public decimal ModifyWeakMultiplier(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature dealer,
        CardModel? cardSource)
    {
        if (dealer != Owner)
        {
            return amount;
        }

        return amount - (1m - amount);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)
        {
            return;
        }

        await PowerCmd.Decrement(this);
    }
}