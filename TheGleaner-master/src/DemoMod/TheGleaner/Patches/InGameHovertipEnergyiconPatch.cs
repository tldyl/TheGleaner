using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Runs;

namespace DemoMod.TheGleaner.Patches;

[HarmonyPatch(typeof(NHoverTipSet), "Init")]
public static class IngameHoverTipEnergyIconPatch
{
    private static Texture2D? _gleanerEnergyTipIcon;

    private static Texture2D? TryLoadTexture(params string[] paths)
    {
        foreach (string path in paths)
        {
            try
            {
                Texture2D? tex = ResourceLoader.Load<Texture2D>(path);
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
                // ignore
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

    private static bool IsInRun()
    {
        return RunManager.Instance != null && RunManager.Instance.IsInProgress;
    }

    private static bool IsTargetPoolCard(Control owner)
    {
        if (owner is not NCardHolder holder)
        {
            return false;
        }

        string? poolName = holder.CardModel?.Pool?.GetType().Name;

        return poolName is
            "ColorlessCardPool" or
            "CurseCardPool" or
            "EventCardPool" or
            "TokenCardPool" or
            "StatusCardPool";
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
            if (!IsInRun())
            {
                return;
            }

            if (!IsTargetPoolCard(owner))
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
        catch
        {
            // swallow to avoid breaking UI
        }
    }
}