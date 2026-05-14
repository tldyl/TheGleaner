using DemoMod.TheGleaner.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace TheGleaner.DemoMod.TheGleaner.Patches;

public class NGamePatch {
    [HarmonyPatch(typeof(NGame), "_Ready")]
    public static class PatchReady {
        public static void Postfix(NGame __instance) {
            __instance.AddChild(new SoundManager());
            SoundKeys.Initialize();
        }
    }
}
