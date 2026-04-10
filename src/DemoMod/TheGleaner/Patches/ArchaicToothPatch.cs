using DemoMod.TheGleaner.Cards.GleanerCard;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace TheGleaner.DemoMod.TheGleaner.Patches;

public class ArchaicToothPatch {
    [HarmonyPatch(typeof(ArchaicTooth), "get_TranscendenceUpgrades")]
    public static class PatchTranscendenceUpgrades {
        public static void Postfix(Dictionary<ModelId, CardModel> __result) {
            __result[ModelDb.Card<Glissando>().Id] = ModelDb.Card<Windmill>();
        }
    }
}
