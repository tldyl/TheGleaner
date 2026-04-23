using System;
using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.RestSiteOptions;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DemoMod.TheGleaner.Relics;

[Pool(typeof(JeraRelicPool))]
public sealed class Telescope : CustomRelicModel
{
    public Telescope()
    {
    }

    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Transform, Array.Empty<DynamicVar>())
    ];

    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (Owner == null)
            return false;

        if (player != Owner)
            return false;

        options.Add(new StargazeRestSiteOption(player));
        return true;
    }
}