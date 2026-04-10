using DemoMod.TheGleaner.Cards.GleanerCard;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace DemoMod.TheGleaner.Patches.ScoreEntryCardPatch;

public class NPlayerHandPatch {
    [HarmonyPatch(typeof(NPlayerHand), "UpdateSelectModeCardVisibility")]
    public static class PatchUpdateSelectModeCardVisibility {
        public static void Postfix(NPlayerHand __instance) {
            NHandCardHolder holder = __instance.ActiveHolders.Where(holder => holder.CardModel is ScoreEntryCard).FirstOrDefault();
            if (holder != null && __instance.CurrentMode is NPlayerHand.Mode.SimpleSelect or NPlayerHand.Mode.UpgradeSelect) {
                holder.Visible = false;
                holder.UpdateCard();
                __instance.ForceRefreshCardIndices();
            }
        }
    }
}
