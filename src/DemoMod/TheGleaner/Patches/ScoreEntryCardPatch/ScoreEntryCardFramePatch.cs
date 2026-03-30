using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace DemoMod.TheGleaner.Patches.ScoreEntryCardPatch;

[HarmonyPatch(typeof(NCard), "Reload")]
public static class ScoreEntryCardFramePatch {
    private static Texture2D? _scoreFrame;

    private static Texture2D? TryLoadTexture(params string[] paths) {
        foreach (string path in paths) {
            try {
                Texture2D? tex = ResourceLoader.Load<Texture2D>(path);
                if (tex != null) {
                    return tex;
                }

                Image image = new Image();
                Error err = image.Load(path);
                if (err == Error.Ok) {
                    return ImageTexture.CreateFromImage(image);
                }
            } catch (Exception) {
            }
        }

        return null;
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

    private static void ClearTextureRect(Node root, params string[] names) {
        TextureRect? target = FindTextureRectByName(root, names);
        if (target == null) {
            return;
        }

        target.Texture = null;
        target.Visible = false;
    }

    private static int HideNodesByNameContains(Node root, params string[] fragments) {
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
            hiddenCount += HideNodesByNameContains(child, fragments);
        }

        return hiddenCount;
    }

    private static void ApplyScoreEntryCardStyle(NCard cardNode) {
        TextureRect? frameNode = FindTextureRectByName(cardNode, "%Frame", "Frame");
        Texture2D? scoreFrame = GetScoreFrame();
        if (frameNode != null) {
            frameNode.Texture = scoreFrame;
            frameNode.Material = null;
        }

        ClearTextureRect(cardNode, "%EnergyIcon", "EnergyIcon");
        ClearTextureRect(cardNode, "%CardBanner", "%Banner", "CardBanner", "Banner");
        ClearTextureRect(cardNode, "%CardPortraitBorder", "%PortraitBorder", "%PortraitFrame", "CardPortraitBorder", "PortraitBorder", "PortraitFrame");

        HideNodesByNameContains(cardNode, "banner");

        HideNodesByNameContains(cardNode, "portrait_border_plaque", "portraitborderplaque", "border_plaque", "plaque");
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(NCard __instance) {
        try {
            if (__instance == null) {
                return;
            }

            string cardId = __instance.Model?.Id?.Entry ?? string.Empty;
            if (!string.Equals(cardId, "DEMOMOD-SCORE_ENTRY_CARD", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            ApplyScoreEntryCardStyle(__instance);
        } catch (Exception) {
        }
    }
}
