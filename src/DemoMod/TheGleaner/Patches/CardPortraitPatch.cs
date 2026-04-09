using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using DemoMod.TheGleaner.Cards.GleanerCard;

namespace DemoMod.TheGleaner.Patches;

[HarmonyPatch(typeof(NCard), "UpdateVisuals")]
public static class ClusterStrikePortraitPatch
{
    [HarmonyPostfix]
    public static void Postfix(NCard __instance)
    {
        if (__instance?.Model is not ClusterStrike model)
            return;

        try
        {
            Texture2D? portrait = model.Portrait;
            if (portrait == null)
                return;

            if (model.Rarity == CardRarity.Ancient)
            {
                TextureRect? ancientPortrait = __instance.GetNodeOrNull<TextureRect>("%AncientPortrait");
                if (ancientPortrait != null && ancientPortrait.Texture != portrait)
                {
                    ancientPortrait.Texture = portrait;
                }
            }
            else
            {
                TextureRect? portraitRect = __instance.GetNodeOrNull<TextureRect>("%Portrait");
                if (portraitRect != null && portraitRect.Texture != portrait)
                {
                    portraitRect.Texture = portrait;
                }
            }
        }
        catch
        {
            // 避免 UI 刷新异常影响整局
        }
    }
}