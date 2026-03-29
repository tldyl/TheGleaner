using BaseLib.Patches.Content;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace DemoMod.TheGleaner.Patches;

public class PlayerCombatStatePatch {
    [HarmonyPatch(typeof(PlayerCombatState), "get_AllPiles")]
    public static class PatchGetAllPiles {
        public static void Postfix(PlayerCombatState __instance, ref IReadOnlyList<CardPile> __result) {
            CardPile pile = CustomPiles.GetCustomPile(__instance, CustomEnums.ScorePile);
            if (pile != null) {
                __result = [.. __result, pile];
            }
        }
    }
}
