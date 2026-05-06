using DemoMod.TheGleaner.Nodes.Vfx;
using DemoMod.TheGleaner.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Nodes;

namespace TheGleaner.DemoMod.TheGleaner.Patches;

public class NGamePatch {
    [HarmonyPatch(typeof(NGame), "_Ready")]
    public static class PatchReady {
        public static void Postfix(NGame __instance) {
            __instance.AddChild(new SoundManager());
            SoundKeys.Initialize();

            NGrayGradientVfxPostProcessor postProcessor = PreloadManager.Cache.GetScene("res://TheGleaner/scenes/vfx/gray_gradient_vfx.tscn").Instantiate<NGrayGradientVfxPostProcessor>();
            __instance.AddChild(postProcessor);
        }
    }
}
