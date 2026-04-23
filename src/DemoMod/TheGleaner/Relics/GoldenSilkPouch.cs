using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Potions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;

namespace DemoMod.TheGleaner.Relics;

/// <summary>
/// 金丝缎口袋：获得该遗物后，立刻获得一瓶点金药水。
/// </summary>
[Pool(typeof(JeraRelicPool))]
public sealed class GoldenSilkPouch : CustomRelicModel
{
    public GoldenSilkPouch()
    {
    }

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPotion<AlchemyGoldPotion>()];

    public override async Task AfterObtained()
    {
        if (Owner == null)
            return;

        await PotionCmd.TryToProcure<AlchemyGoldPotion>(Owner);
    }
}