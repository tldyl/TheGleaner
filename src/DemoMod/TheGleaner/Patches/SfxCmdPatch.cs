using DemoMod.TheGleaner.Nodes.Vfx;
using DemoMod.TheGleaner.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes;

namespace TheGleaner.DemoMod.TheGleaner.Patches;

public class SfxCmdPatch {
    [HarmonyPatch(typeof(SfxCmd), "Play", typeof(string), typeof(float))]
    public static class PatchPlay1 {
        public static bool Prefix(string sfx, float volume) {
            if (!sfx.StartsWith("event:")) {
                if (SoundManager.Instance == null) {
                    Log.Info("Initializing SoundManager and NGrayGradientVfxPostProcessor");
                    NGame.Instance.AddChild(new SoundManager());
                    SoundKeys.Initialize();

                    NGrayGradientVfxPostProcessor postProcessor = PreloadManager.Cache.GetScene("res://TheGleaner/scenes/vfx/gray_gradient_vfx.tscn").Instantiate<NGrayGradientVfxPostProcessor>();
                    NGame.Instance.AddChild(postProcessor);
                }
                SoundManager.Instance.PlaySound(sfx, volume);
                return false;
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(SfxCmd), "Play", typeof(string), typeof(string) , typeof(float), typeof(float))]
    public static class PatchPlay2 {
        public static bool Prefix(string sfx, string param , float val, float volume) {
            if (!sfx.StartsWith("event:")) {
                if (SoundManager.Instance == null) {
                    Log.Info("Initializing SoundManager and NGrayGradientVfxPostProcessor");
                    NGame.Instance.AddChild(new SoundManager());
                    SoundKeys.Initialize();

                    NGrayGradientVfxPostProcessor postProcessor = PreloadManager.Cache.GetScene("res://TheGleaner/scenes/vfx/gray_gradient_vfx.tscn").Instantiate<NGrayGradientVfxPostProcessor>();
                    NGame.Instance.AddChild(postProcessor);
                }
                SoundManager.Instance.PlaySound(sfx, volume);
                return false;
            }
            return true;
        }
    }
}
