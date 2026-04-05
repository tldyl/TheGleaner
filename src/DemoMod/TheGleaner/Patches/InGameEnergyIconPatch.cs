using System;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization.Formatters;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Runs;
using SmartFormat.Core.Extensions;
using DemoMod.TheGleaner;

namespace DemoMod.TheGleaner.Patches;

[HarmonyPatch]
public static class InGameEnergyIconPatch
{
    [ThreadStatic]
    private static bool _isTargetPoolCard;

    private const string GleanerIconTag =
        "[img]res://TheGleaner/images/packed/sprite_fonts/gleaner_energy_icon.png[/img]";

    private static bool IsTargetPoolCard(NCard? card)
    {
        string? poolName = card?.Model?.Pool?.GetType().Name;

        bool result = poolName is
            "ColorlessCardPool" or
            "CurseCardPool" or
            "EventCardPool" or
            "TokenCardPool" or
            "StatusCardPool";

        return result;
    }

    private static bool IsInRun()
    {
        bool inRun = RunManager.Instance != null && RunManager.Instance.IsInProgress;
        return inRun;
    }

    [HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
    public static class TrackCardPatch
    {
        [HarmonyPrefix]
        public static void Prefix(NCard __instance)
        {
            _isTargetPoolCard = IsTargetPoolCard(__instance);
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            _isTargetPoolCard = false;
        }
    }

    [HarmonyPatch(typeof(EnergyIconsFormatter), nameof(EnergyIconsFormatter.TryEvaluateFormat))]
    public static class FormatterPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(IFormattingInfo formattingInfo, ref bool __result)
        {
            if (!_isTargetPoolCard)
            {
                return true;
            }

            if (!IsInRun())
            {
                return true;
            }

            try
            {
                int amount = GetAmount(formattingInfo.CurrentValue, formattingInfo.FormatterOptions);
                string finalText = FormatWithGleanerIcon(amount);

                formattingInfo.Write(finalText);
                __result = true;
                return false;
            }
            catch (Exception e)
            {
                return true;
            }
        }

        private static int GetAmount(object? currentValue, string? formatterOptions)
        {
            if (currentValue is EnergyVar energyVar)
            {
                return Convert.ToInt32(energyVar.PreviewValue);
            }

            if (currentValue is CalculatedVar calculatedVar)
            {
                return Convert.ToInt32(calculatedVar.Calculate(null));
            }

            if (currentValue is decimal dec)
            {
                return (int)dec;
            }

            if (currentValue is int i)
            {
                return i;
            }

            if (currentValue is string && int.TryParse(formatterOptions, out int parsed))
            {
                return parsed;
            }

            throw new InvalidOperationException(
                $"Unsupported value: {currentValue} ({currentValue?.GetType()})");
        }

        private static string FormatWithGleanerIcon(int amount)
        {
            if (amount > 0 && amount < 4)
            {
                return string.Concat(Enumerable.Repeat(GleanerIconTag, amount));
            }

            return $"{amount}{GleanerIconTag}";
        }
    }
}