using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Patches;
public class HookPatch {
    [HarmonyPatch(typeof(Hook), "AfterCardEnteredCombat")]
    public static class PatchAfterCardEnteredCombat {
        public static void Prefix(ref CombatState combatState, CardModel card) {
            if (combatState == null) {
                combatState = card.Owner.Creature.CombatState;
            }
        }
    }
}
