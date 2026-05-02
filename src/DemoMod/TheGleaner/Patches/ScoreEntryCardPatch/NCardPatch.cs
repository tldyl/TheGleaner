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
using MegaCrit.Sts2.Core.Helpers;
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
                    .GetNode<Node2D>("GleanerResonanceGlowIcon").GetNode<TextureRect>("OrbitVfx").Visible = 
                __instance.Model.Keywords.Contains(CustomEnums.Resonance) && __instance.Model.Pile is {IsCombatPile: true};
            __instance.GetNode<TextureRect>("%EnergyIcon")
                .GetNode<Node2D>("GleanerResonanceGlowIcon").GetNode<TextureRect>("HintOrbitVfx").Visible = 
                __instance.Model.Pile is {Type: PileType.Hand} && shouldShowHintIcon(__instance.Model);
            return true;
        }

        public static bool shouldShowHintIcon(CardModel card) {
            if (card.EnergyCost.GetResolved() < 2 || card.EnergyCost.CostsX || card.Keywords.Contains(CustomEnums.Resonance)) {
                return false;
            }
            foreach (CardModel c in card.Owner.PlayerCombatState.Hand.Cards) {
                if (c == card) {
                    continue;
                }
                if (c.Type != card.Type && c.Keywords.Contains(CustomEnums.Resonance)) {
                    return true;
                }
            }
            return false;
        }
        
        private static string getScoreCardIconPath(CardModel? card) {
            if (card == null) {
                return "res://TheGleaner/images/score_card_icon/empty.png";
            }
            if (ResourceLoader.Exists("res://TheGleaner/images/score_card_icon/" + card.Id.Entry.ToLowerInvariant() + "__" + card.Type.ToString().ToLower() + "__" + card.Rarity.ToString().ToLower() + ".png")) {
                return "res://TheGleaner/images/score_card_icon/" + card.Id.Entry.ToLowerInvariant() + "__" + card.Type.ToString().ToLower() + "__" + card.Rarity.ToString().ToLower() + ".png";
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
                __instance.GetNode<TextureRect>("%EnergyIcon").RemoveChild(__instance.GetNode<TextureRect>("%EnergyIcon").GetNode("GleanerResonanceGlowIcon"));
            }
            Node2D gleanerResonanceGlowIcon = PreloadManager.Cache.GetScene("res://TheGleaner/scenes/resonance_glow_icon.tscn").Instantiate<Node2D>();
            TextureRect glowIcon = gleanerResonanceGlowIcon.GetNode<TextureRect>("Icon");
            TextureRect orbitVfx = gleanerResonanceGlowIcon.GetNode<TextureRect>("OrbitVfx");
            TextureRect hintOrbitVfx = gleanerResonanceGlowIcon.GetNode<TextureRect>("HintOrbitVfx");
            //glowIcon.Texture = PreloadManager.Cache.GetTexture2D(getResonanceGlowIconPath(__instance.Model, glowIcon));
            string? orbitVfxPath = getResonanceGlowIconOrbitVfxPath(__instance.Model, orbitVfx);
            string? hintOrbitVfxPath = getResonanceGlowIconHintOrbitVfxPath(__instance.Model, hintOrbitVfx);
            if (orbitVfxPath != null) {
                orbitVfx.Texture = PreloadManager.Cache.GetTexture2D(orbitVfxPath);
            }
            if (hintOrbitVfxPath != null) {
                hintOrbitVfx.Texture = PreloadManager.Cache.GetTexture2D(hintOrbitVfxPath);
            }
            __instance.GetNode<TextureRect>("%EnergyIcon").AddChild(gleanerResonanceGlowIcon);
            gleanerResonanceGlowIcon.GetParent().MoveChild(gleanerResonanceGlowIcon, 0);
        }

        public static void refreshResonanceGlowIcon(NCard nCard) {
            Node2D gleanerResonanceGlowIcon = nCard.GetNode<TextureRect>("%EnergyIcon")
                .GetNode<Node2D>("GleanerResonanceGlowIcon");
            TextureRect glowIcon = gleanerResonanceGlowIcon.GetNode<TextureRect>("Icon");
            TextureRect orbitVfx = gleanerResonanceGlowIcon.GetNode<TextureRect>("OrbitVfx");
            TextureRect hintOrbitVfx = gleanerResonanceGlowIcon.GetNode<TextureRect>("HintOrbitVfx");
            //glowIcon.Texture = PreloadManager.Cache.GetTexture2D(getResonanceGlowIconPath(nCard.Model, glowIcon));
            string? orbitVfxPath = getResonanceGlowIconOrbitVfxPath(nCard.Model, orbitVfx);
            string? hintOrbitVfxPath = getResonanceGlowIconHintOrbitVfxPath(nCard.Model, hintOrbitVfx);
            if (orbitVfxPath != null) {
                orbitVfx.Texture = PreloadManager.Cache.GetTexture2D(orbitVfxPath);
            }
            if (hintOrbitVfxPath != null) {
                hintOrbitVfx.Texture = PreloadManager.Cache.GetTexture2D(hintOrbitVfxPath);
            }
        }
        
        private static string getResonanceGlowIconPath(CardModel? cardModel, TextureRect glowIcon) {
            Player player = LocalContext.GetMe(RunManager.Instance.DebugOnlyGetState());
            if (player == null || cardModel == null) {
                glowIcon.Modulate = Color.FromHtml("26e532c8");
                return "res://images/vfx/dot.png";
            }
            switch (player.Character) {
                case Characters.TheGleaner:
                    glowIcon.Modulate = Color.FromHtml("ffffffff");
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
                    return "res://images/vfx/dot.png";
            }
        }

        private static string? getResonanceGlowIconOrbitVfxPath(CardModel? cardModel, TextureRect glowIcon) {
            Player player = LocalContext.GetMe(RunManager.Instance.DebugOnlyGetState());
            if (player == null || cardModel == null) {
                glowIcon.Modulate = Color.FromHtml("26e532c8");
                return null;
            }
            switch (player.Character) {
                case Characters.TheGleaner:
                    switch (cardModel.Type) {
                        case CardType.Attack:
                            return "res://TheGleaner/images/resonance_energy_icon/R.png";
                        case CardType.Skill:
                            return "res://TheGleaner/images/resonance_energy_icon/B.png";
                        case CardType.Power:
                            return "res://TheGleaner/images/resonance_energy_icon/Y.png";
                        case CardType.None:
                        case CardType.Status:
                        case CardType.Curse:
                        case CardType.Quest:
                        default:
                            return "res://TheGleaner/images/resonance_energy_icon/BLACK.png";
                    }
                default:
                    return null;
            }
        }

        private static string? getResonanceGlowIconHintOrbitVfxPath(CardModel? cardModel, TextureRect glowIcon) {
            Player player = LocalContext.GetMe(RunManager.Instance.DebugOnlyGetState());
            if (player == null || cardModel == null) {
                glowIcon.Modulate = Color.FromHtml("26e532c8");
                return null;
            }
            switch (player.Character) {
                case Characters.TheGleaner:
                    switch (cardModel.Type) {
                        case CardType.Attack:
                            return "res://TheGleaner/images/resonance_energy_icon/R_A.png";
                        case CardType.Skill:
                            return "res://TheGleaner/images/resonance_energy_icon/B_A.png";
                        case CardType.Power:
                            return "res://TheGleaner/images/resonance_energy_icon/Y_A.png";
                        case CardType.None:
                        case CardType.Status:
                        case CardType.Curse:
                        case CardType.Quest:
                        default:
                            return "res://TheGleaner/images/resonance_energy_icon/BLACK_A.png";
                    }
                default:
                    return null;
            }
        }
    }
    
    [HarmonyPatch(typeof(NCard), "Reload")]
    public static class PatchReload {
        public static void Postfix(NCard __instance) {
            if (__instance.Model == null) {
                return;
            }
            PatchReady.Prefix(__instance);
            __instance.GetNode<TextureRect>("%EnergyIcon")
                .GetNode<Node2D>("GleanerResonanceGlowIcon").GetNode<TextureRect>("OrbitVfx").Visible = 
                __instance.Model.Keywords.Contains(CustomEnums.Resonance) && __instance.Model.Pile is {IsCombatPile: true};
            __instance.GetNode<TextureRect>("%EnergyIcon")
                    .GetNode<Node2D>("GleanerResonanceGlowIcon").GetNode<TextureRect>("HintOrbitVfx").Visible = 
                __instance.Model.Pile is {Type: PileType.Hand} && PatchUpdateVisuals.shouldShowHintIcon(__instance.Model);
        }
    }
}
