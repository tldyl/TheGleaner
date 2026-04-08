using DemoMod.TheGleaner.Cards.GleanerCard;
using DemoMod.TheGleaner.Enums;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Runs;

namespace DemoMod.TheGleaner.Patches.ScoreEntryCardPatch;

public class NCardPatch {
    [HarmonyPatch(typeof(NCard), "UpdateVisuals")]
    public static class PatchUpdateVisuals {
        public static bool Prefix(NCard __instance, PileType pileType, CardPreviewMode previewMode) {
            if (__instance.Model is ScoreEntryCard && __instance.IsNodeReady() && pileType != PileType.None) {
                __instance.Model.UpgradePreviewType = CardUpgradePreviewType.Combat;
                foreach (DynamicVar dynamicVar in __instance.Model.DynamicVars.Values)
                    dynamicVar.UpdateCardPreview(__instance.Model, CardPreviewMode.Normal, null, true);
                string str = __instance.Model.GetDescriptionForPile(PileType.Hand);
                MegaRichTextLabel descriptionLines = (MegaRichTextLabel) AccessTools.Field(typeof(NCard), "_descriptionLabel").GetValue(__instance);
                descriptionLines.SetTextAutoSize("[center]" + str + "[/center]");
                return false;
            }
            if (__instance.Model.Pile is {IsCombatPile: true}) {
                PatchReady.refreshResonanceGlowIcon(__instance);
            }
            __instance.GetNode<TextureRect>("%EnergyIcon")
                    .GetNode<TextureRect>("GleanerResonanceGlowIcon").Visible = 
                __instance.Model.Keywords.Contains(CustomEnums.Resonance) && __instance.Model.Pile is {IsCombatPile: true};
            return true;
        }
    }

    [HarmonyPatch(typeof(NCard), "_Ready")]
    public static class PatchReady {
        public static void Prefix(NCard __instance) {
            if (__instance.GetNode<TextureRect>("%EnergyIcon").HasNode("GleanerResonanceGlowIcon")) {
                return;
            }
            TextureRect glowIcon = new TextureRect();
            glowIcon.Texture = PreloadManager.Cache.GetTexture2D(getResonanceGlowIconPath(__instance.Model, glowIcon));
            glowIcon.Size = new Vector2(256, 256);
            glowIcon.Position = new Vector2(-96, -96);
            glowIcon.PivotOffset = new Vector2(128, 128);
            glowIcon.Material = new CanvasItemMaterial();
            glowIcon.Name = "GleanerResonanceGlowIcon";
            ((CanvasItemMaterial)glowIcon.Material).BlendMode = CanvasItemMaterial.BlendModeEnum.Add;
            __instance.GetNode<TextureRect>("%EnergyIcon").AddChild(glowIcon);
            glowIcon.GetParent().MoveChild(glowIcon, 0);
            glowIcon.Visible = false;
        }

        public static void refreshResonanceGlowIcon(NCard nCard) {
            TextureRect glowIcon = nCard.GetNode<TextureRect>("%EnergyIcon")
                .GetNode<TextureRect>("GleanerResonanceGlowIcon");
            glowIcon.Texture = PreloadManager.Cache.GetTexture2D(getResonanceGlowIconPath(nCard.Model, glowIcon));
        }
        
        private static string getResonanceGlowIconPath(CardModel? cardModel, TextureRect glowIcon) {
            Player player = LocalContext.GetMe(RunManager.Instance.DebugOnlyGetState());
            if (player == null || cardModel == null) {
                glowIcon.Modulate = Color.FromHtml("26e532c8");
                return "res://images/vfx/dot.png";
            }
            switch (player.Character) {
                case Characters.TheGleaner:
                    switch (cardModel.Type) {
                        case CardType.Attack:
                            return "res://TheGleaner/images/resonance_energy_icon/energy_colorless_rl.png";
                        case CardType.Skill:
                            return "res://TheGleaner/images/resonance_energy_icon/energy_colorless_bl.png";
                        case CardType.Power:
                        case CardType.None:
                        case CardType.Status:
                        case CardType.Curse:
                        case CardType.Quest:
                        default:
                            return "res://TheGleaner/images/resonance_energy_icon/energy_colorless_yl.png";
                    }
                default:
                    glowIcon.Modulate = Color.FromHtml("26e532c8");
                    return "res://Images/vfx/dot.png";
            }
        }
    }
    
    [HarmonyPatch(typeof(NCard), "Reload")]
    public static class PatchReload {
        public static void Postfix(NCard __instance) {
            if (__instance.Model == null) {
                return;
            }
            if (!__instance.GetNode<TextureRect>("%EnergyIcon").HasNode("GleanerResonanceGlowIcon")) {
                PatchReady.Prefix(__instance);
            }
            __instance.GetNode<TextureRect>("%EnergyIcon")
                .GetNode<TextureRect>("GleanerResonanceGlowIcon").Visible = 
                __instance.Model.Keywords.Contains(CustomEnums.Resonance) && __instance.Model.Pile is {IsCombatPile: true};
        }
    }
}
