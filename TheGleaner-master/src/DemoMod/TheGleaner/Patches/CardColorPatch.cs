using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
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

        // 保险：有些项目里 Id 可能会写成去掉下划线的形式
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

    private static Texture2D? GetScoreFrame() {
        return _scoreFrame ??= TryLoadTexture(
            "res://TheGleaner/images/packed/sprite_fonts/Score.png",
            "res://TheGleaner/images/packed/sprite_fonts/score.png",
            "res://images/packed/sprite_fonts/Score.png",
            "res://images/packed/sprite_fonts/score.png"
        );
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

    private static void ClearTextureRect(Node root, string cardId, string label, params string[] names) {
        TextureRect? target = FindTextureRectByName(root, names);
        if (target == null) {
            return;
        }

        target.Texture = null;
        target.Visible = false;
    }

    private static int HideNodesByNameContains(Node root, string cardId, string label, params string[] fragments) {
        int hiddenCount = 0;
        string nodeName = root.Name.ToString();
        bool shouldHide = false;
        for (int i = 0; i < fragments.Length; i++) {
            if (nodeName.Contains(fragments[i], StringComparison.OrdinalIgnoreCase)) {
                shouldHide = true;
                break;
            }
        }

        if (shouldHide && root is CanvasItem canvasItem) {
            canvasItem.Visible = false;
            switch (root) {
                case TextureRect textureRect:
                    textureRect.Texture = null;
                    break;
                case Sprite2D sprite2D:
                    sprite2D.Texture = null;
                    break;
                case NinePatchRect ninePatchRect:
                    ninePatchRect.Texture = null;
                    break;
            }
            hiddenCount++;
        }

        foreach (Node child in root.GetChildren()) {
            hiddenCount += HideNodesByNameContains(child, cardId, label, fragments);
        }

        return hiddenCount;
    }

    private static void ApplyScoreEntryCardStyle(NCard cardNode, string cardId) {
        TextureRect? frameNode = FindTextureRectByName(cardNode, "%Frame", "Frame");
        Texture2D? scoreFrame = GetScoreFrame();
        if (frameNode == null) {
        } else {
            frameNode.Texture = scoreFrame;
            frameNode.Material = null;
        }

        ClearTextureRect(cardNode, cardId, "energy icon", "%EnergyIcon", "EnergyIcon");
        ClearTextureRect(cardNode, cardId, "card banner", "%CardBanner", "%Banner", "CardBanner", "Banner");
        ClearTextureRect(cardNode, cardId, "card portrait border", "%CardPortraitBorder", "%PortraitBorder", "%PortraitFrame", "CardPortraitBorder", "PortraitBorder", "PortraitFrame");

        int bannerHidden = HideNodesByNameContains(cardNode, cardId, "card banner fallback", "banner");
        if (bannerHidden == 0) {
        }

        int plaqueHidden = HideNodesByNameContains(cardNode, cardId, "card portrait border plaque", "portrait_border_plaque", "portraitborderplaque", "border_plaque", "plaque");
        if (plaqueHidden == 0) {
        }

        HideLabelText(cardNode, cardId, "title", "%TitleLabel", "TitleLabel");
        HideLabelText(cardNode, cardId, "energy cost text", "%EnergyLabel", "EnergyLabel");
    }

    private static void HideLabelText(Node root, string cardId, string label, params string[] names) {
        foreach (string name in names) {
            var megaLabel = root.GetNodeOrNull<MegaLabel>(name);
            if (megaLabel != null) {
                megaLabel.SetTextAutoSize(string.Empty);
                megaLabel.Visible = false;
                return;
            }
        }

        foreach (Node child in root.GetChildren()) {
            HideLabelTextRecursive(child, cardId, label, names);
        }
    }

    private static bool HideLabelTextRecursive(Node node, string cardId, string label, params string[] names) {
        if (node is MegaLabel megaLabel) {
            string nodeName = megaLabel.Name.ToString();
            for (int i = 0; i < names.Length; i++) {
                string candidate = names[i].TrimStart('%');
                if (string.Equals(nodeName, candidate, StringComparison.OrdinalIgnoreCase)) {
                    megaLabel.SetTextAutoSize(string.Empty);
                    megaLabel.Visible = false;
                    return true;
                }
            }
        }

        foreach (Node child in node.GetChildren()) {
            if (HideLabelTextRecursive(child, cardId, label, names)) {
                return true;
            }
        }

        return false;
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

            // 这些牌完全跳过，不受本 patch 影响
            if (IsExcluded(cardId)) {
                return;
            }

            if (string.Equals(cardId, "DEMOMOD-SCORE_ENTRY_CARD", StringComparison.OrdinalIgnoreCase)) {
                ApplyScoreEntryCardStyle(__instance, cardId);
                return;
            }

            if (!cardId.StartsWith("DEMOMOD-", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            TextureRect? frameNode = __instance.GetNodeOrNull<TextureRect>("%Frame");
            if (frameNode == null) {
            } else {
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
            if (energyNode == null) {
            } else {
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