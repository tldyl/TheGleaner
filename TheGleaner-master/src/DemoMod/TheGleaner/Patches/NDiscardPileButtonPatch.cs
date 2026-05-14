using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace DemoMod.TheGleaner.Patches;

public class NDiscardPileButtonPatch {
    [HarmonyPatch(typeof(NDiscardPileButton), "_Ready")]
    public static class PatchReady {
        public static void Postfix(NDiscardPileButton __instance) {
            RunState runState = (RunState) AccessTools.PropertyGetter(typeof(RunManager), "State").Invoke(RunManager.Instance, []);
            Player player = LocalContext.GetMe(runState.Players);
            if (player is {Character: Characters.TheGleaner}) {
                AccessTools.Field(typeof(NCombatCardPile), "_emptyPileMessage").SetValue(__instance, new LocString("combat_messages", "JERA_OPEN_EMPTY_DISCARD"));
            }
        }
    }
}
