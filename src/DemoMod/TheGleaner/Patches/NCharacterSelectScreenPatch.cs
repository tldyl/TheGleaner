using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Models;
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

    [HarmonyPatch(typeof(NCharacterSelectScreen), "SelectCharacter")]
    public static class PatchSelectCharacter {
        public static void Postfix(NCharacterSelectScreen __instance, NCharacterSelectButton charSelectButton, CharacterModel characterModel) {
            if (!__instance.HasNode("SkinSelectPanel")) {
                VBoxContainer skinSelectPanel = PreloadManager.Cache.GetScene("res://TheGleaner/scenes/skin_select_panel.tscn").Instantiate<VBoxContainer>();
                skinSelectPanel.Position = new Vector2(220, 192);
                __instance.AddChild(skinSelectPanel);
            }
            __instance.GetNode<VBoxContainer>("SkinSelectPanel").Visible = characterModel is global::DemoMod.TheGleaner.Characters.TheGleaner;
        }
    }
}
