using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace DemoMod.TheGleaner.Patches;

[HarmonyPatch(typeof(NCard), "UpdateVisuals")]
public static class TypePlaqueRepairPatch
{
    private static Texture2D? _defaultTypePlaque;

    [HarmonyPostfix]
    public static void Postfix(NCard __instance)
    {
        try
        {
            if (__instance?.Model == null)
                return;

            string cardId = __instance.Model.Id?.Entry ?? string.Empty;

            // ScoreEntryCard 继续允许你走自己的特殊显示
            if (string.Equals(cardId, "DEMOMOD-SCORE_ENTRY_CARD", StringComparison.OrdinalIgnoreCase))
                return;

            TextureRect? plaque = FindTypePlaque(__instance);
            if (plaque == null)
                return;

            // 先恢复可见性
            plaque.Visible = true;

            // 如果纹理已经正常，就不碰
            if (plaque.Texture != null)
                return;

            // 只有在 texture 丢失时才补默认 plaque
            Texture2D? defaultPlaque = GetDefaultTypePlaque();
            if (defaultPlaque != null)
            {
                plaque.Texture = defaultPlaque;
            }
        }
        catch
        {
            // 避免 UI 修复影响整局
        }
    }

    private static Texture2D? GetDefaultTypePlaque()
    {
        return _defaultTypePlaque ??= TryLoadTexture(
            // 你提到的资源名
            "res://scenes/cards/card_portrait_border_plaque_s.tres",

            // 再给几个常见兜底，防止项目里路径不一样
            "res://card_portrait_border_plaque_s.tres",
            "res://TheGleaner/card_portrait_border_plaque_s.tres",
            "res://TheGleaner/images/card_portrait_border_plaque_s.tres"
        );
    }

    private static Texture2D? TryLoadTexture(params string[] paths)
    {
        foreach (string path in paths)
        {
            try
            {
                Texture2D? tex = ResourceLoader.Load<Texture2D>(path);
                if (tex != null)
                    return tex;
            }
            catch
            {
            }
        }

        return null;
    }

    private static TextureRect? FindTypePlaque(Node root)
    {
        // 先查常见唯一名/节点名
        TextureRect? direct =
            root.GetNodeOrNull<TextureRect>("%TypePlaque")
            ?? root.GetNodeOrNull<TextureRect>("TypePlaque")
            ?? root.GetNodeOrNull<TextureRect>("%CardPortraitBorderPlaque")
            ?? root.GetNodeOrNull<TextureRect>("CardPortraitBorderPlaque")
            ?? root.GetNodeOrNull<TextureRect>("%PortraitBorderPlaque")
            ?? root.GetNodeOrNull<TextureRect>("PortraitBorderPlaque");

        if (direct != null)
            return direct;

        return FindTypePlaqueRecursive(root);
    }

    private static TextureRect? FindTypePlaqueRecursive(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is TextureRect rect)
            {
                string name = rect.Name.ToString();

                if (name.Contains("TypePlaque", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("PortraitBorderPlaque", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("CardPortraitBorderPlaque", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("portrait_border_plaque", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("border_plaque", StringComparison.OrdinalIgnoreCase))
                {
                    return rect;
                }
            }

            TextureRect? nested = FindTypePlaqueRecursive(child);
            if (nested != null)
                return nested;
        }

        return null;
    }
}