using DemoMod.TheGleaner.Cards.GleanerCard;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using System.Reflection;

namespace DemoMod.TheGleaner.Patches;

public class CardModelPatch {
    [HarmonyPatch]
    public static class PatchGetDescriptionForPile {
        static MethodBase TargetMethod() {
            foreach (MethodInfo method in typeof(CardModel).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
                if (method.Name == "GetDescriptionForPile" && method.GetParameters().Length == 3) {
                    return method;
                }
            }
            return null;
        }

        public static void Postfix(CardModel __instance, ref string __result, PileType pileType, Enum previewType, Creature? target) {
            if (__instance is IAppendDescriptionCard appendDescriptionCard) {
                __result += appendDescriptionCard.AppendDescription();
            }
        }
    }
}
