using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace DemoMod.TheGleaner.Patches;

[HarmonyPatch(typeof(NCard), "Reload")]
public static class CardColorPatch {
    private static readonly string[] _forceColorlessStatusFrameCardIds =
    [
        "DEMOMOD-DIRGE_OF_FAREWELL",
        "DEMOMOD-HOWL_OF_WRATH",
        "DEMOMOD-SHRIEK_OF_DREAD",
        "DEMOMOD-DIRGEOFFAREWELL",
        "DEMOMOD-HOWLOFWRATH",
        "DEMOMOD-SHRIEKOFDREAD"
    ];

    // 这几张牌强制恢复成游戏原版 colorless 外观
    private static readonly string[] _forceVanillaColorlessCardIds =
    [
        "DEMOMOD-ZERO_COST_ATTACKS",
        "DEMOMOD-ONE_COST_ATTACKS",
        "DEMOMOD-TWO_COST_ATTACKS",
        "DEMOMOD-THREE_OR_MORE_COST_ATTACKS",

        // 保险：兼容去下划线写法
        "DEMOMOD-ZEROCOSTATTACKS",
        "DEMOMOD-ONECOSTATTACKS",
        "DEMOMOD-TWOCOSTATTACKS",
        "DEMOMOD-THREEORMORECOSTATTACKS"
    ];

    // 不受这个 patch 影响的卡
    private static readonly string[] _excludedCardIds =
    [
        "DEMOMOD-SHRIEK_OF_DREAD",
        "DEMOMOD-HOWL_OF_WRATH",
        "DEMOMOD-DIRGE_OF_FAREWELL",
        "DEMOMOD-SHRIEKOFDREAD",
        "DEMOMOD-HOWLOFWRATH",
        "DEMOMOD-DIRGEOFFAREWELL",

        "DEMOMOD-NIGHTINGALE_AT_THE_ABYSS",
        "DEMOMOD-FORGING_AT_DAWN",
        "DEMOMOD-PULSATION_OF_THE_TIDES",
        "DEMOMOD-SWAP_PILES",
        "DEMOMOD-SHUFFLE",
        "DEMOMOD-GLEAN_CARD",
        "DEMOMOD-CLUSTER_STRIKE",

        // 保险：兼容去下划线
        "DEMOMOD-NIGHTINGALEATTHEABYSS",
        "DEMOMOD-FORGINGATDAWN",
        "DEMOMOD-PULSATIONOFTHETIDES",
        "DEMOMOD-SWAPPILES",
        "DEMOMOD-GLEANCARD",
        "DEMOMOD-CLUSTERSTRIKE"
    ];

    private static Texture2D? _attackIcon;
    private static Texture2D? _skillIcon;
    private static Texture2D? _powerIcon;

    private static Texture2D? _attackFrame;
    private static Texture2D? _skillFrame;
    private static Texture2D? _powerFrame;
    private static Texture2D? _scoreFrame;

    // 原版 colorless 模板卡缓存
    private static CardModel? _vanillaColorlessTemplateCard;

    private static Texture2D? TryLoadTexture(params string[] paths) {
        foreach (string path in paths) {
            try {
                var tex = ResourceLoader.Load<Texture2D>(path);
                if (tex != null) {
                    return tex;
                }

                Image image = new Image();
                Error err = image.Load(path);
                if (err == Error.Ok) {
                    return ImageTexture.CreateFromImage(image);
                }
            } catch (Exception e) {
            }
        }

        return null;
    }

    private static bool IsExcluded(string cardId) {
        for (int i = 0; i < _excludedCardIds.Length; i++) {
            if (string.Equals(_excludedCardIds[i], cardId, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }

        return false;
    }

    private static bool ShouldForceVanillaColorless(string cardId) {
        for (int i = 0; i < _forceVanillaColorlessCardIds.Length; i++) {
            if (string.Equals(_forceVanillaColorlessCardIds[i], cardId, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }

        return false;
    }

    private static Texture2D? GetEnergyIcon(string type) {
        return type switch {
            "Attack" => _attackIcon ??= TryLoadTexture(
                "res://TheGleaner/images/packed/sprite_fonts/energy_colorless_r.png",
                "res://images/packed/sprite_fonts/energy_colorless_r.png"
            ),
            "Skill" => _skillIcon ??= TryLoadTexture(
                "res://TheGleaner/images/packed/sprite_fonts/energy_colorless_b.png",
                "res://images/packed/sprite_fonts/energy_colorless_b.png"
            ),
            "Power" => _powerIcon ??= TryLoadTexture(
                "res://TheGleaner/images/packed/sprite_fonts/energy_colorless_y.png",
                "res://images/packed/sprite_fonts/energy_colorless_y.png"
            ),
            _ => null
        };
    }

    private static Texture2D? GetCardFrame(string type) {
        return type switch {
            "Attack" => _attackFrame ??= TryLoadTexture(
                "res://TheGleaner/images/packed/sprite_fonts/card_frame_attack_s.png",
                "res://images/packed/sprite_fonts/card_frame_attack_s.png"
            ),
            "Skill" => _skillFrame ??= TryLoadTexture(
                "res://TheGleaner/images/packed/sprite_fonts/card_frame_skill_s.png",
                "res://images/packed/sprite_fonts/card_frame_skill_s.png"
            ),
            "Power" => _powerFrame ??= TryLoadTexture(
                "res://TheGleaner/images/packed/sprite_fonts/card_frame_power_s.png",
                "res://images/packed/sprite_fonts/card_frame_power_s.png"
            ),
            _ => null
        };
    }

    private static bool ShouldForceColorlessStatusFrame(string cardId) {
        for (int i = 0; i < _forceColorlessStatusFrameCardIds.Length; i++) {
            if (string.Equals(_forceColorlessStatusFrameCardIds[i], cardId, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }

        return false;
    }


    private static CardModel? GetVanillaColorlessTemplateCard() {
        if (_vanillaColorlessTemplateCard != null) {
            return _vanillaColorlessTemplateCard;
        }

        try {
            // 用原版 colorless 攻击牌当模板
            // FlashOfSteel / SecretWeapon / Finesse 都行，优先选攻击牌
            _vanillaColorlessTemplateCard = ModelDb.Card<FlashOfSteel>();
            if (_vanillaColorlessTemplateCard != null) {
                return _vanillaColorlessTemplateCard;
            }
        } catch (Exception e) {
        }

        try {
            _vanillaColorlessTemplateCard = ModelDb.Card<SecretWeapon>();
            if (_vanillaColorlessTemplateCard != null) {
                return _vanillaColorlessTemplateCard;
            }
        } catch (Exception e) {
        }

        try {
            _vanillaColorlessTemplateCard = ModelDb.Card<Finesse>();
            if (_vanillaColorlessTemplateCard != null) {
                return _vanillaColorlessTemplateCard;
            }
        } catch (Exception e) {
        }

        return null;
    }

    private static TextureRect? FindTextureRectByName(Node root, params string[] names) {
        for (int i = 0; i < names.Length; i++) {
            TextureRect? byUnique = root.GetNodeOrNull<TextureRect>(names[i]);
            if (byUnique != null) {
                return byUnique;
            }
        }

        foreach (Node child in root.GetChildren()) {
            if (child is TextureRect rect) {
                for (int i = 0; i < names.Length; i++) {
                    string candidate = names[i].TrimStart('%');
                    if (string.Equals(rect.Name.ToString(), candidate, StringComparison.OrdinalIgnoreCase)) {
                        return rect;
                    }
                }
            }

            TextureRect? nested = FindTextureRectByName(child, names);
            if (nested != null) {
                return nested;
            }
        }

        return null;
    }



    private static void ApplyVanillaColorlessStyle(NCard cardNode) {
        CardModel? template = GetVanillaColorlessTemplateCard();
        if (template == null) {
            return;
        }

        TextureRect? frameNode = FindTextureRectByName(cardNode, "%Frame", "Frame");
        TextureRect? energyNode = FindTextureRectByName(cardNode, "%EnergyIcon", "EnergyIcon");
        TextureRect? bannerNode = FindTextureRectByName(cardNode, "%TitleBanner", "TitleBanner", "%Banner", "Banner");
        TextureRect? portraitBorderNode = FindTextureRectByName(cardNode, "%PortraitBorder", "PortraitBorder");

        if (energyNode != null) {
            energyNode.Texture = template.EnergyIcon;
            energyNode.Visible = template.EnergyIcon != null;
        }

        if (frameNode != null) {
            frameNode.Texture = template.Frame;
            frameNode.Material = template.FrameMaterial;
            frameNode.Visible = true;
        }

        if (bannerNode != null) {
            bannerNode.Texture = template.BannerTexture;
            bannerNode.Material = template.BannerMaterial;
            bannerNode.Visible = true;
        }

        if (portraitBorderNode != null) {
            portraitBorderNode.Texture = template.PortraitBorder;
            portraitBorderNode.Material = template.BannerMaterial;
            portraitBorderNode.Visible = true;
        }
    }



    [HarmonyPostfix]
    public static void Postfix(NCard __instance) {
        try {
            var model = __instance?.Model;
            if (model == null) {
                return;
            }

            string cardId = model.Id?.Entry ?? "<null-id>";
            string type = model.Type.ToString();

            if (IsExcluded(cardId)) {
                return;
            }


            if (!cardId.StartsWith("DEMOMOD-", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            // 这 4 张牌强制改成游戏原版 colorless 外观
            if (ShouldForceVanillaColorless(cardId)) {
                ApplyVanillaColorlessStyle(__instance);
                return;
            }

            TextureRect? frameNode = __instance.GetNodeOrNull<TextureRect>("%Frame");
            if (frameNode != null) {
                string frameType = type;
                if (string.Equals(type, "Status", StringComparison.OrdinalIgnoreCase)
                    && ShouldForceColorlessStatusFrame(cardId)) {
                    frameType = "Skill";
                }

                Texture2D? frameTex = GetCardFrame(frameType);
                if (frameTex != null) {
                    frameNode.Texture = frameTex;
                }
                frameNode.Material = null;
            }

            TextureRect? energyNode = __instance.GetNodeOrNull<TextureRect>("%EnergyIcon");
            if (energyNode != null) {
                Texture2D? energyTex = GetEnergyIcon(type);
                if (energyTex != null) {
                    energyNode.Texture = energyTex;
                }
            }
        } catch (Exception e) {
        }
    }
}

[HarmonyPatch(typeof(NCard), "UpdateVisuals")]
public static class CardVisualTextPatch {
    [HarmonyPostfix]
    public static void Postfix(NCard __instance) {
        try {
            var model = __instance?.Model;
            string cardId = model?.Id?.Entry ?? string.Empty;
            if (!string.Equals(cardId, "DEMOMOD-SCORE_ENTRY_CARD", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var title = __instance.GetNodeOrNull<MegaLabel>("%TitleLabel");
            if (title != null) {
                title.SetTextAutoSize(string.Empty);
                title.Visible = false;
            }

            var energy = __instance.GetNodeOrNull<MegaLabel>("%EnergyLabel");
            if (energy != null) {
                energy.SetTextAutoSize(string.Empty);
                energy.Visible = false;
            }

        } catch (Exception e) {
        }
    }
    
}
