using MegaCrit.Sts2.Core.Context;
using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace DemoMod.TheGleaner.Relics;

[Pool(typeof(JeraRelicPool))]
public sealed class Handkerchief : CustomRelicModel
{
    public Handkerchief()
    {
    }

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal originalPrice)
    {
        if (Owner == null)
            return originalPrice;

        if (player != Owner)
            return originalPrice;

        if (!LocalContext.IsMe(Owner))
            return originalPrice;

        if (entry is not MerchantCardRemovalEntry)
            return originalPrice;

        return 50m;
    }
}