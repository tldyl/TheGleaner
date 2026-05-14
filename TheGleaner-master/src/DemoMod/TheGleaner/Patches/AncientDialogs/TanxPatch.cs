using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;

namespace TheGleaner.DemoMod.TheGleaner.Patches.AncientDialogs;

public class TanxPatch {
    [HarmonyPatch(typeof(Tanx), "DefineDialogues")]
    public static class PatchDefineDialogues {
        public static void Postfix(Tanx __instance, AncientDialogueSet __result) {
            __result.CharacterDialogues[ModelDb.Character<global::DemoMod.TheGleaner.Characters.TheGleaner>().Id.Entry] = [
                new AncientDialogue(["event:/sfx/npcs/tanx/tanx_curiosity", "", "event:/sfx/npcs/tanx/tanx_laugh", "", "event:/sfx/npcs/tanx/tanx_roar"]) {
                    VisitIndex = 0
                }
            ];
        }
    }
}
