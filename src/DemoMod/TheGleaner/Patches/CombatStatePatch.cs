using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;

namespace TheGleaner.DemoMod.TheGleaner.Patches;

public class CombatStatePatch {
    [HarmonyPatch(typeof(CombatState), "IterateHookListeners")]
    public static class PatchIterateHookListeners {
        public static void Postfix(CombatState __instance, ref IEnumerable<AbstractModel> __result) {
            List<AbstractModel> ret = [];
            ret.AddRange(__result);
            ret.Reverse();
            ret.AddRange(__instance.Players.Select(player => player.Character));
            ret = ret.Distinct().ToList();
            __result = ret;
        }
    }
}
