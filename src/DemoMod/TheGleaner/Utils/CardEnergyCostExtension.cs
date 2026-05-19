using DemoMod.TheGleaner.Enums;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Utils;

public static class CardEnergyCostExtension {
    public static void SetUntilEnterDiscardPile(this CardEnergyCost cardEnergyCost, int cost, bool reduceOnly = false) {
        if (cost == 0 && cardEnergyCost.Canonical < 0)
            return;
        List<LocalCostModifier> _localModifiers = AccessTools.Field(typeof(CardEnergyCost), "_localModifiers").GetValue(cardEnergyCost) as List<LocalCostModifier>;
        _localModifiers.Add(new LocalCostModifier(cost, LocalCostType.Absolute, CustomEnums.WhenEnterDiscardPile | LocalCostModifierExpiration.WhenPlayed, reduceOnly));
    }

    public static bool AfterCardEnterDiscardPileCleanup(this CardEnergyCost cardEnergyCost) {
        CardModel _card = AccessTools.Field(typeof(CardEnergyCost), "_card").GetValue(cardEnergyCost) as CardModel;
        List<LocalCostModifier> _localModifiers = AccessTools.Field(typeof(CardEnergyCost), "_localModifiers").GetValue(cardEnergyCost) as List<LocalCostModifier>;
        _card.AssertMutable();
        return _localModifiers.RemoveAll((Predicate<LocalCostModifier>) (m => m.Expiration.HasFlag(CustomEnums.WhenEnterDiscardPile))) > 0;
    }
}
