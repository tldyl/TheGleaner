using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Relics;
[Pool(typeof(RelicPool))]
public class Jera : CustomRelicModel {
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override string PackedIconPath => "res://TheGleaner/images/relics/Jera.png";
    protected override string PackedIconOutlinePath => "res://TheGleaner/images/relics/Jera.png";
    protected override string BigIconPath => "res://TheGleaner/images/relics/Jera.png";

    private int counter;
    private bool _showCounter;
    public override bool ShowCounter => _showCounter;
    public override int DisplayAmount => counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Draws", 1)];

    public override async Task BeforeCombatStart() {
        _showCounter = true;
        counter = 0;
        InvokeDisplayAmountChanged();
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState) {
        if (side == CombatSide.Player) {
            counter++;
            InvokeDisplayAmountChanged();
            switch (counter) {
                case 1:
                    await CardPileCmd.Draw(choiceContext, DynamicVars["Draws"].BaseValue, Owner);
                    break;
            }
        }
    }

    public override async Task AfterEnergyReset(Player player) {
        if (counter == 2) {
            await PlayerCmd.GainEnergy(1, Owner);
        }
    }

    public override async Task AfterCombatEnd(CombatRoom room) {
        counter = 0;
        _showCounter = false;
        InvokeDisplayAmountChanged();
    }
}
