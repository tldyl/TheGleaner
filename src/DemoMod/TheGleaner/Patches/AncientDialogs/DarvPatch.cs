using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;

namespace TheGleaner.DemoMod.TheGleaner.Patches.AncientDialogs;

public class DarvPatch {
    [HarmonyPatch(typeof(Darv), "DefineDialogues")]
    public static class PatchDefineDialogues {
        public static void Postfix(Darv __instance, AncientDialogueSet __result) {
            __result.CharacterDialogues[ModelDb.Character<global::DemoMod.TheGleaner.Characters.TheGleaner>().Id.Entry] = [
                new AncientDialogue(["event:/sfx/npcs/darv/darv_introduction", "", "event:/sfx/npcs/darv/darv_endeared"]) {
                    VisitIndex = 0
                }
            ];
        }
    }
}
