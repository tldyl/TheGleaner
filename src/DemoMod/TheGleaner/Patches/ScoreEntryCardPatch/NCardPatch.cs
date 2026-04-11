using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Cards.GleanerCard;
using DemoMod.TheGleaner.Commands;
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
                AccessTools.Method(typeof(NCard), "UpdateStarCostVisuals", [typeof(PileType)]).Invoke(__instance, [pileType]);
                __instance.Model.UpgradePreviewType = CardUpgradePreviewType.Combat;
                foreach (DynamicVar dynamicVar in __instance.Model.DynamicVars.Values)
                    dynamicVar.UpdateCardPreview(__instance.Model, CardPreviewMode.Normal, null, true);
                string str = __instance.Model.GetDescriptionForPile(PileType.Hand);
                MegaRichTextLabel descriptionLines = (MegaRichTextLabel) AccessTools.Field(typeof(NCard), "_descriptionLabel").GetValue(__instance);
                descriptionLines.SetTextAutoSize("[center]" + str + "[/center]");
                //乐谱内牌的缩略图显示
                if (!__instance.HasNode("ScrollContainer")) {
                    ScrollContainer scrollContainer = new ScrollContainer();
                    scrollContainer.Name = "ScrollContainer";
                    scrollContainer.HorizontalScrollMode = ScrollContainer.ScrollMode.ShowNever;
                    scrollContainer.VerticalScrollMode = ScrollContainer.ScrollMode.ShowNever;
                    scrollContainer.CustomMinimumSize = new Vector2(252.0f, 0.0f);
                    scrollContainer.Size = new Vector2(268, 204);
                    scrollContainer.Position = new Vector2(-128, -164);
                    
                    HFlowContainer flowContainer = new HFlowContainer();
                    flowContainer.Name = "FlowContainer";
                    flowContainer.CustomMinimumSize = new Vector2(268.0f, 0.0f);
                    
                    scrollContainer.AddChild(flowContainer);
                    __instance.AddChild(scrollContainer);
                }
                HFlowContainer flowContainer2 = __instance.GetNode<HFlowContainer>("ScrollContainer/FlowContainer");
                foreach (Node child in flowContainer2.GetChildren()) {
                    child.QueueFree();
                }
                ScorePile scorePile = ScorePileCmd.GetOrCreateScorePile(__instance.Model.Owner.PlayerCombatState);
                foreach (CardModel card in scorePile.Cards) {
                    TextureRect cardIcon = new TextureRect();
                    cardIcon.Texture = PreloadManager.Cache.GetTexture2D(getScoreCardIconPath(card));
                    cardIcon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
                    cardIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                    cardIcon.CustomMinimumSize = new Vector2(50.0f, 78.0f);
                    flowContainer2.AddChild(cardIcon);
                }
                for (int _ = 0; _ < ScorePileCmd.GetCapacity(__instance.Model.Owner) - scorePile.Cards.Count; _++) {
                    TextureRect cardIcon = new TextureRect();
                    cardIcon.Texture = PreloadManager.Cache.GetTexture2D(getScoreCardIconPath(null));
                    cardIcon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
                    cardIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                    cardIcon.CustomMinimumSize = new Vector2(50.0f, 78.0f);
                    flowContainer2.AddChild(cardIcon);
                }
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

        private static string getScoreCardIconPath(CardModel? card) {
            if (card == null) {
                return "res://TheGleaner/images/score_card_icon/empty.png";
            }
            if (card is ClusterStrike) {
                return "res://TheGleaner/images/score_card_icon/cluster_strike.png";
            }
            if (card.Tags.Contains(CardTag.Strike)) {
                return "res://TheGleaner/images/score_card_icon/strike.png";
            }
            if (card.Tags.Contains(CardTag.Defend)) {
                return "res://TheGleaner/images/score_card_icon/defend.png";
            }
            return card.Type switch {
                CardType.Attack => "res://TheGleaner/images/score_card_icon/attack.png",
                CardType.Skill => "res://TheGleaner/images/score_card_icon/skill.png",
                CardType.Power => "res://TheGleaner/images/score_card_icon/power.png",
                CardType.Status => "res://TheGleaner/images/score_card_icon/status.png",
                CardType.Curse or CardType.Quest => "res://TheGleaner/images/score_card_icon/curse.png",
                _ => "res://TheGleaner/images/score_card_icon/unknown.png"
            };
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
