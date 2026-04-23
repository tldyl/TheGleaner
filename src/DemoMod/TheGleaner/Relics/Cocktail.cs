using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enchantments;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace DemoMod.TheGleaner.Relics;

/// <summary>
/// 鸡尾酒：所有打击和防御附魔“艾黎的碎霜”。
/// </summary>
[Pool(typeof(JeraRelicPool))]
public sealed class Cocktail : CustomRelicModel
{
    public Cocktail()
    {
    }

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromEnchantment<LiyasShatteredFrost>(1);

    public override Task AfterObtained()
    {
        if (Owner == null)
            return Task.CompletedTask;

        var deckCards = PileType.Deck.GetPile(Owner).Cards.ToList();

        foreach (var card in deckCards)
        {
            bool isStrike = card.Tags.Contains(CardTag.Strike);
            bool isDefend = card.Tags.Contains(CardTag.Defend);

            if (!isStrike && !isDefend)
                continue;

            if (!ModelDb.Enchantment<LiyasShatteredFrost>().CanEnchant(card))
                continue;

            CardCmd.Enchant<LiyasShatteredFrost>(card, 1m);

            var vfx = NCardEnchantVfx.Create(card);
            if (vfx != null)
            {
                NRun.Instance?.GlobalUi.CardPreviewContainer?.AddChild(vfx);
            }
        }

        return Task.CompletedTask;
    }
}