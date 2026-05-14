using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace DemoMod.TheGleaner.Patches;

public class NDrawPileButtonPatch {
    [HarmonyPatch(typeof(NDrawPileButton), "_Ready")]
    public static class PatchReady {
        public static void Postfix(NDrawPileButton __instance) {
            RunState runState = (RunState) AccessTools.PropertyGetter(typeof(RunManager), "State").Invoke(RunManager.Instance, []);
            Player player = LocalContext.GetMe(runState.Players);
            if (player is {Character: Characters.TheGleaner}) {
                AccessTools.Field(typeof(NCombatCardPile), "_emptyPileMessage").SetValue(__instance, new LocString("combat_messages", "JERA_OPEN_EMPTY_DRAW"));
            }
        }
    }
}
