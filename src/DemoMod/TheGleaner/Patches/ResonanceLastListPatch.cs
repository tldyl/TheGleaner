using System;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.Cards;
using DemoMod.TheGleaner.Enums;

namespace DemoMod.TheGleaner.Patches;

[HarmonyPatch(typeof(NCard), "UpdateVisuals")]
public static class ResonanceDescriptionOrderFallbackPatch
{
    public static void Postfix(NCard __instance)
    {
        try
        {
            if (__instance?.Model == null)
                return;

            if (!__instance.Model.Keywords.Contains(CustomEnums.Resonance))
                return;

            MegaRichTextLabel descriptionLabel =
                AccessTools.Field(typeof(NCard), "_descriptionLabel").GetValue(__instance) as MegaRichTextLabel;

            if (descriptionLabel == null)
                return;

            string oldText = descriptionLabel.Text;
            if (string.IsNullOrWhiteSpace(oldText))
                return;

            string newText = MoveResonanceBlockToEnd(oldText);

            if (!string.Equals(oldText, newText, StringComparison.Ordinal))
            {
                descriptionLabel.SetTextAutoSize(newText);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"[TheGleaner] ResonanceDescriptionOrderFallbackPatch failed: {e}");
        }
    }

    private static string MoveResonanceBlockToEnd(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        // 保留 [center] ... [/center]
        bool hasCenterPrefix = text.StartsWith("[center]", StringComparison.OrdinalIgnoreCase);
        bool hasCenterSuffix = text.EndsWith("[/center]", StringComparison.OrdinalIgnoreCase);

        string body = text;
        if (hasCenterPrefix)
            body = body.Substring("[center]".Length);
        if (hasCenterSuffix && body.Length >= "[/center]".Length)
            body = body.Substring(0, body.Length - "[/center]".Length);

        string normalized = body.Replace("\r\n", "\n").Replace("\r", "\n");
        string[] lines = normalized.Split('\n');

        var resonanceLines = lines
            .Where(IsResonanceLine)
            .ToList();

        if (resonanceLines.Count == 0)
            return text;

        var otherLines = lines
            .Where(line => !IsResonanceLine(line))
            .ToList();

        // 把 resonance 相关行拼到最后
        var finalLines = otherLines.Concat(resonanceLines).ToArray();
        string rebuilt = string.Join("\n", finalLines);

        if (hasCenterPrefix)
            rebuilt = "[center]" + rebuilt;
        if (hasCenterSuffix)
            rebuilt += "[/center]";

        return rebuilt;
    }

    private static bool IsResonanceLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return false;

        string plain = StripBbCode(line).Trim();

        // 这里做宽松匹配，适配中英文显示
        // 你如果游戏里实际显示的关键词名不是这些，就把实际文本加进来
        return Regex.IsMatch(
            plain,
            @"(^|\b)(resonance|共鸣)($|\b)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
        );
    }

    private static string StripBbCode(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // 去掉类似 [gold] [/gold] [img]...[/img] [keyword] 之类标记
        string s = Regex.Replace(input, @"\[(\/)?[^\]]+\]", "");
        return s;
    }
}