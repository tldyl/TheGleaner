using DemoMod.TheGleaner.Cards.GleanerCard;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace DemoMod.TheGleaner.Patches.ScoreEntryCardPatch;

public class NCardPatch {
    [HarmonyPatch(typeof(NCard), "UpdateVisuals")]
    public static class PatchUpdateVisuals {
        public static bool Prefix(NCard __instance, PileType pileType, CardPreviewMode previewMode) {
            if (__instance.Model is ScoreEntryCard && __instance.IsNodeReady() && pileType != PileType.None) {
                __instance.Model.UpgradePreviewType = CardUpgradePreviewType.Combat;
                foreach (DynamicVar dynamicVar in __instance.Model.DynamicVars.Values)
                    dynamicVar.UpdateCardPreview(__instance.Model, CardPreviewMode.Normal, null, true);
                string str = __instance.Model.GetDescriptionForPile(PileType.Hand);
                MegaRichTextLabel descriptionLines = (MegaRichTextLabel) AccessTools.Field(typeof(NCard), "_descriptionLabel").GetValue(__instance);
                descriptionLines.SetTextAutoSize("[center]" + str + "[/center]");
                return false;
            }
            return true;
        }
    }
}
