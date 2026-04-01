using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;

namespace TheGleaner.DemoMod.TheGleaner.Patches.AncientDialogs;

public class OrobasPatch {
    [HarmonyPatch(typeof(Orobas), "DefineDialogues")]
    public static class PatchDefineDialogues {
        public static void Postfix(Orobas __instance, AncientDialogueSet __result) {
            __result.CharacterDialogues[ModelDb.Character<global::DemoMod.TheGleaner.Characters.TheGleaner>().Id.Entry] = [
                new AncientDialogue(["", "", "", "", ""]) {
                    VisitIndex = 0
                }
            ];
        }
    }
}
