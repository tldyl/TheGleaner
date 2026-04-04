using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace DemoMod.TheGleaner.Patches;

[HarmonyPatch(typeof(NHoverTipSet), "Init")]
public static class HoverTipEnergyIconPatch
{
    private static Texture2D? _gleanerEnergyTipIcon;

    private static Texture2D? TryLoadTexture(params string[] paths)
    {
        foreach (string path in paths)
        {
            try
            {
                var tex = ResourceLoader.Load<Texture2D>(path);
                if (tex != null)
                {
                    return tex;
                }

                Image image = new Image();
                Error err = image.Load(path);
                if (err == Error.Ok)
                {
                    return ImageTexture.CreateFromImage(image);
                }
            }
            catch
            {
            }
        }

        return null;
    }

    private static Texture2D? GetGleanerEnergyTipIcon()
    {
        return _gleanerEnergyTipIcon ??= TryLoadTexture(
            "res://TheGleaner/images/packed/sprite_fonts/gleaner_energy_icon.png",
            "res://images/packed/sprite_fonts/gleaner_energy_icon.png"
        );
    }

    private static bool IsGleanerOwner(Control owner)
    {
        if (owner is not NCardHolder holder)
        {
            return false;
        }

        string cardId = holder.CardModel?.Id?.Entry ?? string.Empty;
        return cardId.StartsWith("DEMOMOD-", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEnergyHoverTip(IHoverTip hoverTip)
    {
        return hoverTip.Id != null
               && hoverTip.Id.Contains("static_hover_tips.ENERGY.title", StringComparison.OrdinalIgnoreCase);
    }

    [HarmonyPostfix]
    public static void Postfix(NHoverTipSet __instance, Control owner, IEnumerable<IHoverTip> hoverTips)
    {
        try
        {
            if (!IsGleanerOwner(owner))
            {
                return;
            }

            Texture2D? replacement = GetGleanerEnergyTipIcon();
            if (replacement == null)
            {
                return;
            }

            Node? textContainer = __instance.GetNodeOrNull("textHoverTipContainer");
            if (textContainer == null)
            {
                return;
            }

            int textTipIndex = 0;

            foreach (IHoverTip hoverTip in IHoverTip.RemoveDupes(hoverTips))
            {
                if (hoverTip is not HoverTip)
                {
                    continue;
                }

                Node? child = textContainer.GetChildOrNull<Node>(textTipIndex);
                textTipIndex++;

                if (child == null)
                {
                    continue;
                }

                if (!IsEnergyHoverTip(hoverTip))
                {
                    continue;
                }

                TextureRect? iconNode = child.GetNodeOrNull<TextureRect>("%Icon");
                if (iconNode != null)
                {
                    iconNode.Texture = replacement;
                }
            }
        }
        catch (Exception e)
        {
        }
    }
}