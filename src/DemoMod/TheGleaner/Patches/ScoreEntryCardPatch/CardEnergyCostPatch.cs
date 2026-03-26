using DemoMod.TheGleaner.Cards.GleanerCard;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Patches.ScoreEntryCardPatch;

public class CardEnergyCostPatch {
    [HarmonyPatch(typeof(CardEnergyCost), "GetWithModifiers")]
    public static class PatchGetWithModifiers {
        public static bool Prefix(CardEnergyCost __instance, ref int __result) {
            CardModel _card = AccessTools.Field(typeof(CardEnergyCost), "_card").GetValue(__instance) as CardModel;
            if (_card is ScoreEntryCard) {
                __result = 0;
                return false;
            }
            return true;
        }
    }
}
