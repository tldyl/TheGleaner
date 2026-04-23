using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace TheGleaner.DemoMod.TheGleaner.Patches;

public class ChecksumTrackerPatch {
    [HarmonyPatch(typeof(ChecksumTracker), "CompareChecksums")]
    public static class PatchCompareChecksums {
        public static bool Prefix(ChecksumTracker __instance) {
            return false;
        }
    }
}
