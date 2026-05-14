using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Patches;

public class CombatHistoryPatch {
    [HarmonyPatch(typeof(CombatHistory), "CardGenerated")]
    public static class PatchCardGenerated {
        public static void Prefix(CombatHistory __instance, ref CombatState combatState, CardModel card, bool generatedByPlayer) {
            if (combatState == null) {
                combatState = card.Owner.Creature.CombatState;
            }
        }
    }
}
