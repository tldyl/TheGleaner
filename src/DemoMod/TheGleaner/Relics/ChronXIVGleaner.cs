using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace DemoMod.TheGleaner.Relics;

[Pool(typeof(JeraRelicPool))]
public class ChronXIVGleaner : JeraExclusiveRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override string PackedIconPath => "res://TheGleaner/images/relics/demomod-chron_xi_v_gleaner.png";
    protected override string PackedIconOutlinePath => "res://TheGleaner/images/relics/demomod-chron_xi_v_gleaner.png";
    protected override string BigIconPath => "res://TheGleaner/images/relics/demomod-chron_xi_v_gleaner.png";

    private int counter;
    private bool _showCounter;

    public override bool ShowCounter => _showCounter;
    public override int DisplayAmount => counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("DrawTurn1", 2),
        new IntVar("DrawTurn2", 1),
        new IntVar("EnergyTurn2", 1),
        new IntVar("EnergyTurn3", 2)
    ];

    public override async Task BeforeCombatStart()
    {
        _showCounter = true;
        counter = 0;
        InvokeDisplayAmountChanged();
    }

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner)
        {
            return count;
        }

        int? roundNumber = player.Creature?.CombatState?.RoundNumber;
        if (roundNumber == null)
        {
            return count;
        }

        if (roundNumber == 1)
        {
            return count + DynamicVars["DrawTurn1"].BaseValue;
        }

        if (roundNumber == 2)
        {
            return count + DynamicVars["DrawTurn2"].BaseValue;
        }

        return count;
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
    {
        if (side != CombatSide.Player)
        {
            return;
        }

        counter++;
        InvokeDisplayAmountChanged();
    }

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner)
        {
            return;
        }

        if (counter == 2)
        {
            await PlayerCmd.GainEnergy(DynamicVars["EnergyTurn2"].BaseValue, Owner);
        }
        else if (counter == 3)
        {
            await PlayerCmd.GainEnergy(DynamicVars["EnergyTurn3"].BaseValue, Owner);
        }
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        counter = 0;
        _showCounter = false;
        InvokeDisplayAmountChanged();
    }
}