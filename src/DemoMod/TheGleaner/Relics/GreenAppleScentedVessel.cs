using System.Threading.Tasks;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Commands;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace DemoMod.TheGleaner.Relics;

[Pool(typeof(JeraRelicPool))]
public sealed class GreenAppleScentedVessel : JeraExclusiveRelic {
    public override RelicRarity Rarity => RelicRarity.Rare;
    public override string PackedIconPath => "res://TheGleaner/images/relics/demomod-green_apple_scented_vessel.png";
    protected override string PackedIconOutlinePath => "res://TheGleaner/images/relics/demomod-green_apple_scented_vessel_outline.png";
    protected override string BigIconPath => "res://TheGleaner/images/relics/demomod-green_apple_scented_vessel.png";

    public override Task BeforeCombatStart() {
        if (Owner != null && IsOwnerJera()) {
            ScorePileCmd.InitializeCapacityFromCurrentDeck(Owner);
        }

        return Task.CompletedTask;
    }

    public override async Task AfterEnergyReset(Player player) {
        if (Owner == null || player != Owner || !IsOwnerJera()) {
            return;
        }

        ScorePile scorePile = CustomPiles.GetCustomPile(player.PlayerCombatState, CustomEnums.ScorePile) as ScorePile;
        if (scorePile == null) {
            return;
        }

        int capacity = ScorePileCmd.GetCapacity(player);
        if (capacity <= 0 || scorePile.Cards.Count < capacity) {
            return;
        }

        Flash();
        await PlayerCmd.GainEnergy(1, player);
    }
}
