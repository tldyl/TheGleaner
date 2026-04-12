using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;

namespace TheGleaner.DemoMod.TheGleaner.Patches;

public class HandPosHelperPatch {
    [HarmonyPatch(typeof(HandPosHelper), "GetPosition")]
    public static class PatchGetPosition {
        public static bool Prefix(int handSize, int cardIndex, ref Vector2 __result) {
            if (cardIndex >= 10) {
                __result = new Vector2(740f, 71f);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(HandPosHelper), "GetAngle")]
    public static class PatchGetAngle {
        public static bool Prefix(int handSize, int cardIndex, ref float __result) {
            if (cardIndex >= 10) {
                __result = 18f;
                return false;
            }
            return true;
        }
    }
}
