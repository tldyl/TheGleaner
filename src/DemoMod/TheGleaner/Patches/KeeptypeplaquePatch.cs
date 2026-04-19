using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace DemoMod.TheGleaner.Patches;

[HarmonyPatch(typeof(NCard), "Reload")]
public static class NCardStateResetPatch
{
    [HarmonyPrefix]
    public static void Prefix(NCard __instance)
    {
        try
        {
            if (__instance == null)
                return;

            RestoreCommonCardUiState(__instance);
        }
        catch (Exception)
        {
            // 不要让 UI 修复 patch 影响整局
        }
    }

    private static void RestoreCommonCardUiState(Node root)
    {
        RestoreNamedTextureRect(root,
            "%EnergyIcon", "EnergyIcon",
            "%CardBanner", "%Banner", "CardBanner", "Banner",
            "%CardPortraitBorder", "%PortraitBorder", "%PortraitFrame",
            "CardPortraitBorder", "PortraitBorder", "PortraitFrame",
            "%Frame", "Frame",
            "%AncientPortrait", "AncientPortrait",
            "%Portrait", "Portrait"
        );

        RestoreNodesByNameContains(root,
            "plaque",
            "banner",
            "portrait_border",
            "portraitborder",
            "border_plaque",
            "portrait_border_plaque",
            "typeplaque",
            "type_plaque"
        );

        RestoreMegaLabel(root, "%TitleLabel", "TitleLabel");
        RestoreMegaLabel(root, "%EnergyLabel", "EnergyLabel");
    }

    private static void RestoreNamedTextureRect(Node root, params string[] names)
    {
        foreach (string name in names)
        {
            TextureRect? rect = root.GetNodeOrNull<TextureRect>(name);
            if (rect != null)
            {
                rect.Visible = true;
            }
        }

        foreach (Node child in root.GetChildren())
        {
            RestoreNamedTextureRectRecursive(child, names);
        }
    }

    private static void RestoreNamedTextureRectRecursive(Node node, params string[] names)
    {
        if (node is TextureRect rect)
        {
            string nodeName = rect.Name.ToString();
            foreach (string raw in names)
            {
                string candidate = raw.TrimStart('%');
                if (string.Equals(nodeName, candidate, StringComparison.OrdinalIgnoreCase))
                {
                    rect.Visible = true;
                    break;
                }
            }
        }

        foreach (Node child in node.GetChildren())
        {
            RestoreNamedTextureRectRecursive(child, names);
        }
    }

    private static void RestoreNodesByNameContains(Node node, params string[] fragments)
    {
        string nodeName = node.Name.ToString();

        foreach (string fragment in fragments)
        {
            if (nodeName.Contains(fragment, StringComparison.OrdinalIgnoreCase))
            {
                if (node is CanvasItem canvasItem)
                    canvasItem.Visible = true;
                break;
            }
        }

        foreach (Node child in node.GetChildren())
        {
            RestoreNodesByNameContains(child, fragments);
        }
    }

    private static void RestoreMegaLabel(Node root, params string[] names)
    {
        foreach (string name in names)
        {
            MegaLabel? label = root.GetNodeOrNull<MegaLabel>(name);
            if (label != null)
            {
                label.Visible = true;
            }
        }

        foreach (Node child in root.GetChildren())
        {
            RestoreMegaLabelRecursive(child, names);
        }
    }

    private static void RestoreMegaLabelRecursive(Node node, params string[] names)
    {
        if (node is MegaLabel label)
        {
            string nodeName = label.Name.ToString();
            foreach (string raw in names)
            {
                string candidate = raw.TrimStart('%');
                if (string.Equals(nodeName, candidate, StringComparison.OrdinalIgnoreCase))
                {
                    label.Visible = true;
                    break;
                }
            }
        }

        foreach (Node child in node.GetChildren())
        {
            RestoreMegaLabelRecursive(child, names);
        }
    }
}