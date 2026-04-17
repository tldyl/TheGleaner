using BaseLib.Patches.Content;
using DemoMod.TheGleaner.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace TheGleaner.DemoMod.TheGleaner.Patches;
public class RunManagerPatch {
    [HarmonyPatch(typeof(RunManager), "CleanUp")]
    public static class PatchCleanUp {
        public static void Prefix(RunManager __instance, bool graceful) {
            Player player = LocalContext.GetMe(__instance.DebugOnlyGetState().Players);
            CustomPiles.Piles.Set(player.PlayerCombatState, null);
        }
        
        public static void Postfix(RunManager __instance, bool graceful) {
            RandomDissonanceCard.initPool();
            CustomPiles.CustomPileProviders.Remove(CustomEnums.ScorePile);
        }
    }
}
