using DemoMod.TheGleaner.Cards.GleanerCard;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
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

    [HarmonyPatch(typeof(CardModel), "EnqueueManualPlay")]
    public static class PatchEnqueueManualPlay {
        public static bool Prefix(CardModel __instance, Creature target) {
            if (__instance is ScoreEntryCard) {
                CardPlay cardPlay = new CardPlay {
                    Card = __instance,
                    Target = target,
                    ResultPile = PileType.Hand,
                    Resources = new ResourceInfo {
                        EnergySpent = 0,
                        EnergyValue = 0,
                        StarsSpent = 0,
                        StarValue = 0
                    },
                    IsAutoPlay = true,
                    PlayIndex = 0,
                    PlayCount = 1
                };
                TaskHelper.RunSafely((Task)AccessTools.Method(typeof(CardModel), "OnPlay", [typeof(PlayerChoiceContext), typeof(CardPlay)]).Invoke(__instance, [new BlockingPlayerChoiceContext(), cardPlay]));
                return false;
            }
            return true;
        }
    }
}
