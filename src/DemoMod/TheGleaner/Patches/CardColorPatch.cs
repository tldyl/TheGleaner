using System;
using Godot;
using HarmonyLib;
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

    private static Texture2D? _attackIcon;
    private static Texture2D? _skillIcon;
    private static Texture2D? _powerIcon;

    private static Texture2D? _attackFrame;
    private static Texture2D? _skillFrame;
    private static Texture2D? _powerFrame;

    private static Texture2D? TryLoadTexture(params string[] paths) {
        foreach (string path in paths) {
            try {
                var tex = ResourceLoader.Load<Texture2D>(path);
                if (tex != null) {
                    return tex;
                }

                // Fallback for local/dev when raw file is available.
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

    [HarmonyPostfix]
    public static void Postfix(NCard __instance) {
        try {
            if (__instance == null) {
                return;
            }

            var model = __instance.Model;
            if (model == null) {
                return;
            }

            string cardId = model.Id?.Entry ?? "<null-id>";
            string type = model.Type.ToString();

            if (!cardId.StartsWith("DEMOMOD-", StringComparison.OrdinalIgnoreCase)) {
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
        } catch (Exception) {
        }
    }
}
