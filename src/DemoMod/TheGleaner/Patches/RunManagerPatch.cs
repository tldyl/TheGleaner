using DemoMod.TheGleaner.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

namespace TheGleaner.DemoMod.TheGleaner.Patches;
public class RunManagerPatch {
    [HarmonyPatch(typeof(RunManager), "CleanUp")]
    public static class PatchCleanUp {
        public static void Postfix(RunManager __instance, bool graceful) {
            RandomDissonanceCard.initPool();
        }
    }
}
