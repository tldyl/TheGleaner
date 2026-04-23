using BaseLib.Abstracts;
using DemoMod.TheGleaner.Potions;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Pools;

public class PotionPool : CustomPotionPoolModel
{
    public override string EnergyColorName => "ironclad";

    protected override IEnumerable<PotionModel> GenerateAllPotions()
    {
        return
        [
            ModelDb.Potion<AlchemyGoldPotion>(),
        ];
    }
}