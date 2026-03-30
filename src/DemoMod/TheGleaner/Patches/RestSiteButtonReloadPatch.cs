using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace AquaIroncladMod;

[HarmonyPatch]
public static class RestSiteButtonReloadPatch
{
    static MethodBase? TargetMethod()
    {
        var method = AccessTools.Method(typeof(NRestSiteButton), "Reload");
        FileLogger.Write("[RestSiteButtonReloadPatch] TargetMethod = " + (method?.ToString() ?? "null"));
        return method;
    }

    [HarmonyPostfix]
    public static void Postfix(NRestSiteButton __instance)
    {
        try
        {
            if (__instance.Option.OptionId != "AQUA_STARGAZE")
                return;

            FileLogger.Write("[RestSiteButtonReloadPatch] Reload postfix for AQUA_STARGAZE");
            ApplyVisualsDelayed(__instance);
        }
        catch (Exception ex)
        {
            FileLogger.Write("[RestSiteButtonReloadPatch] EXCEPTION: " + ex);
        }
    }

    private static async void ApplyVisualsDelayed(NRestSiteButton button)
    {
        try
        {
            for (int i = 0; i < 5; i++)
            {
                if (!GodotObject.IsInstanceValid(button))
                    return;

                var tree = button.GetTree();
                if (tree != null)
                    await button.ToSignal(tree, SceneTree.SignalName.ProcessFrame);

                if (TryApplyVisuals(button))
                    return;
            }

            FileLogger.Write("[RestSiteButtonReloadPatch] Failed to apply visuals after retries");
        }
        catch (Exception ex)
        {
            FileLogger.Write("[RestSiteButtonReloadPatch] ApplyVisualsDelayed EXCEPTION: " + ex);
        }
    }

    private static bool TryApplyVisuals(NRestSiteButton button)
    {
        try
        {
            TextureRect? iconRect = null;
            MegaLabel? label = null;

            try
            {
                iconRect = button.GetNodeOrNull<TextureRect>("%Icon");
            }
            catch
            {
            }

            try
            {
                label = button.GetNodeOrNull<MegaLabel>("%Label");
            }
            catch
            {
            }

            if (label == null || iconRect == null)
            {
                FileLogger.Write(
                    "[RestSiteButtonReloadPatch] UI nodes not ready yet. " +
                    $"label={(label == null ? "null" : "ok")} " +
                    $"icon={(iconRect == null ? "null" : "ok")}"
                );
                return false;
            }

            label.SetTextAutoSize("观星");
            FileLogger.Write("[RestSiteButtonReloadPatch] Label replaced");

            string[] candidatePaths =
            {
                "res://AquaIronclad/images/ui/rest_site/option_aqua_stargaze.png",
                "res://images/ui/rest_site/option_aqua_stargaze.png"
            };

            Texture2D? tex = null;
            string? usedPath = null;

            foreach (string path in candidatePaths)
            {
                FileLogger.Write("[RestSiteButtonReloadPatch] Trying image path = " + path);

                var image = new Image();
                var err = image.Load(path);
                if (err == Error.Ok)
                {
                    tex = ImageTexture.CreateFromImage(image);
                    usedPath = path;
                    break;
                }

                FileLogger.Write("[RestSiteButtonReloadPatch] Image.Load failed: " + path + " err=" + err);
            }

            if (tex == null)
            {
                FileLogger.Write("[RestSiteButtonReloadPatch] Failed to load icon from all candidate paths");
                return false;
            }

            iconRect.Texture = tex;
            FileLogger.Write("[RestSiteButtonReloadPatch] Icon replaced from " + usedPath);
            return true;
        }
        catch (Exception ex)
        {
            FileLogger.Write("[RestSiteButtonReloadPatch] TryApplyVisuals EXCEPTION: " + ex);
            return false;
        }
    }
}