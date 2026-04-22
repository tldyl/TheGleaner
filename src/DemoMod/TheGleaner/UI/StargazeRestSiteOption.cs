using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.RestSiteOptions;

public sealed class StargazeRestSiteOption : RestSiteOption
{
    private const int CardsToTransform = 2;

    public override string OptionId => "DEMOMOD_STARGAZE";

    public override LocString Description
    {
        get
        {
            if (IsEnabled)
            {
                LocString locString = new LocString("rest_site_ui", "OPTION_" + OptionId + ".description");
                locString.Add("Cards", CardsToTransform);
                return locString;
            }

            return new LocString("rest_site_ui", "OPTION_" + OptionId + ".descriptionDisabled");
        }
    }

    public StargazeRestSiteOption(Player owner)
        : base(owner)
    {
        IsEnabled = GetTransformCandidateCount(owner) >= CardsToTransform;
    }

    public override async Task<bool> OnSelect()
    {
        CardSelectorPrefs prefs = new CardSelectorPrefs(
            CardSelectorPrefs.TransformSelectionPrompt,
            CardsToTransform
        )
        {
            Cancelable = true,
            RequireManualConfirmation = true
        };

        IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromDeckForTransformation(Owner, prefs);
        if (!selectedCards.Any())
            return false;

        foreach (CardModel card in selectedCards)
        {
            await CardCmd.TransformToRandom(card, Owner.RunState.Rng.Niche);
        }

        return true;
    }

    private static int GetTransformCandidateCount(Player player)
    {
        return PileType.Deck.GetPile(player).Cards.Count(c => c.IsRemovable);
    }
}