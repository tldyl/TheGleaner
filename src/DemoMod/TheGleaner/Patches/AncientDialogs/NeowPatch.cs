using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;

namespace TheGleaner.DemoMod.TheGleaner.Patches.AncientDialogs;

public class NeowPatch {
    [HarmonyPatch(typeof(Neow), "DefineDialogues")]
    public static class PatchDefineDialogues {
        public static void Postfix(Neow __instance, AncientDialogueSet __result) {
            __result.CharacterDialogues[ModelDb.Character<global::DemoMod.TheGleaner.Characters.TheGleaner>().Id.Entry] = [
                new AncientDialogue(["event:/sfx/npcs/neow/neow_welcome", "", "event:/sfx/npcs/neow/neow_sleepy"]) {
                    VisitIndex = 0
                },
                new AncientDialogue(["event:/sfx/npcs/neow/neow_welcome", ""]) {
                    VisitIndex = 1
                }
            ];
        }
    }
}
