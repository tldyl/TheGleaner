using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace TheGleaner.DemoMod.TheGleaner.Patches;
public class NCharacterSelectScreenPatch {
    [HarmonyPatch(typeof(NCharacterSelectScreen), "_Ready")]
    public static class PatchReady {
        public static void Postfix(NCharacterSelectScreen __instance) {
            RichTextLabel relicDescription = __instance.GetNode<RichTextLabel>("%InfoPanel/VBoxContainer/Relic/Description");
            relicDescription.Size = new Vector2(520, 95);
        }
    }
}
