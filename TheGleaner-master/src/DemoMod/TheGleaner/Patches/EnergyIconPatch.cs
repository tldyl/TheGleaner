using System;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization.Formatters;
using MegaCrit.Sts2.Core.Nodes.Cards;
using SmartFormat.Core.Extensions;

namespace DemoMod.TheGleaner.Patches;

[HarmonyPatch]
public static class EnergyIconPatch
{
    private static bool _isGleanerCard = false;

    private const string IconTag =
        "[img]res://TheGleaner/images/packed/sprite_fonts/gleaner_energy_icon.png[/img]";

    private static bool IsGleanerCard(NCard? card)
    {
        string id = card?.Model?.Id?.Entry ?? string.Empty;
        return id.StartsWith("DEMOMOD-", StringComparison.OrdinalIgnoreCase);
    }

    [HarmonyPatch(typeof(NCard), "UpdateVisuals")]
    public static class TrackCardPatch
    {
        [HarmonyPrefix]
        public static void Prefix(NCard __instance)
        {
            _isGleanerCard = IsGleanerCard(__instance);
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            _isGleanerCard = false;
        }
    }

    [HarmonyPatch(typeof(EnergyIconsFormatter), nameof(EnergyIconsFormatter.TryEvaluateFormat))]
    public static class FormatterPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(EnergyIconsFormatter __instance, IFormattingInfo formattingInfo, ref bool __result)
        {
            try
            {
                if (!_isGleanerCard)
                {
                    return true;
                }

                string? text = null;
                object? currentValue = formattingInfo.CurrentValue;
                int amount;

                EnergyVar? energyVar = currentValue as EnergyVar;
                if (energyVar == null)
                {
                    CalculatedVar? calculatedVar = currentValue as CalculatedVar;
                    if (calculatedVar == null)
                    {
                        if (currentValue is decimal dec)
                        {
                            amount = (int)dec;
                        }
                        else if (currentValue is int i)
                        {
                            amount = i;
                        }
                        else
                        {
                            string? str = currentValue as string;
                            if (str == null)
                            {
                                DefaultInterpolatedStringHandler handler = new DefaultInterpolatedStringHandler(22, 2);
                                handler.AppendLiteral("Unknown value='");
                                handler.AppendFormatted<object?>(formattingInfo.CurrentValue);
                                handler.AppendLiteral("' type=");
                                object? currentValue2 = formattingInfo.CurrentValue;
                                handler.AppendFormatted<Type?>(currentValue2 != null ? currentValue2.GetType() : null);
                            }

                            if (!int.TryParse(formattingInfo.FormatterOptions, out amount))
                            {
                                __result = false;
                                return false;
                            }

                            text = str;
                        }
                    }
                    else
                    {
                        amount = Convert.ToInt32(calculatedVar.Calculate(null));
                    }
                }
                else
                {
                    amount = Convert.ToInt32(energyVar.PreviewValue);
                    if (!string.IsNullOrEmpty(energyVar.ColorPrefix))
                    {
                        text = energyVar.ColorPrefix;
                    }
                }

                if (amount <= 0)
                {
                    amount = 1;
                }

                string finalText;
                if (amount > 0 && amount < 4)
                {
                    finalText = string.Concat(Enumerable.Repeat(IconTag, amount));
                }
                else
                {
                    DefaultInterpolatedStringHandler handler = new DefaultInterpolatedStringHandler(0, 2);
                    handler.AppendFormatted(amount);
                    handler.AppendFormatted(IconTag);
                    finalText = handler.ToStringAndClear();
                }

                formattingInfo.Write(finalText);
                __result = true;
                return false;
            }
            catch
            {
                return true;
            }
        }
    }
}