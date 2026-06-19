using DemoMod.TheGleaner.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;

namespace TheGleaner.DemoMod.TheGleaner.Patches;

public class SfxCmdPatch {
    [HarmonyPatch(typeof(SfxCmd), "Play", typeof(string), typeof(float))]
    public static class PatchPlay1 {
        public static bool Prefix(string sfx, float volume) {
            if (!sfx.StartsWith("event:")) {
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
                SoundManager.Instance.PlaySound(sfx, volume);
                return false;
            }
            return true;
        }
    }
}
