using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using System.Reflection;

namespace DemoMod.TheGleaner.Patches;

public class UnplayableReasonExtensionsPatch {
    [HarmonyPatch]
    public static class PatchGetPlayerDialogueLine {
        static MethodBase TargetMethod() {
            Type patchedType = typeof(UnplayableReason).Assembly.GetType("MegaCrit.Sts2.Core.Entities.Cards.UnplayableReasonExtensions");
            return patchedType.GetMethod("GetPlayerDialogueLine");
        }

        public static void Postfix(UnplayableReason reason, AbstractModel preventer, ref LocString __result) {
            RunState runState = (RunState) AccessTools.PropertyGetter(typeof(RunManager), "State").Invoke(RunManager.Instance, []);
            Player player = LocalContext.GetMe(runState.Players);
            if (player is {Character: Characters.TheGleaner}) {
                AccessTools.Field(typeof(LocString), "<locEntryKey>P").SetValue(__result, "JERA_" + __result.LocEntryKey); 
            }
        }
    }
}
