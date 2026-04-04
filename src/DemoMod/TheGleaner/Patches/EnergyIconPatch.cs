using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization.Formatters;
using MegaCrit.Sts2.Core.Nodes.Cards;
using DemoMod.TheGleaner;

namespace DemoMod.TheGleaner.Patches;

[HarmonyPatch]
public static class EnergyIconPatch
{
    private static bool _isGleanerCard = false;

    private const string IconTag =
        "[img]res://TheGleaner/images/packed/sprite_fonts/gleaner_energy_icon.png[/img]";

    private static bool IsGleanerCard(NCard card)
    {
        string id = card?.Model?.Id?.Entry ?? "";
        return id.StartsWith("DEMOMOD-", StringComparison.OrdinalIgnoreCase);
    }

    // =============================
    // 🔹 捕捉当前卡上下文
    // =============================
    [HarmonyPatch(typeof(NCard), "UpdateVisuals")]
    public static class TrackCardPatch
    {
        [HarmonyPrefix]
        public static void Prefix(NCard __instance)
        {
            _isGleanerCard = IsGleanerCard(__instance);


        }
    }

    // =============================
    // 🔹 替换 energy icon
    // =============================
    [HarmonyPatch(typeof(EnergyIconsFormatter), "TryEvaluateFormat")]
    public static class FormatterPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(object __0, ref bool __result)
        {
            try
            {
   

                if (!_isGleanerCard)
                {
                    return true;
                }

                int amount = 1;

                var type = __0.GetType();

                object? value = type.GetProperty("CurrentValue")?.GetValue(__0);

                if (value is int i)
                    amount = i;
                else if (value is decimal d)
                    amount = (int)d;

                var opt = type.GetProperty("FormatterOptions")?.GetValue(__0) as string;
                if (!string.IsNullOrEmpty(opt) && int.TryParse(opt, out int parsed))
                    amount = parsed;



                if (amount <= 0)
                    amount = 1;

                string result =
                    amount < 4
                        ? string.Concat(Enumerable.Repeat(IconTag, amount))
                        : $"{amount}{IconTag}";

                // ❌ 原来
// var write = type.GetMethod("Write");

// ✅ 正确
var write = type.GetMethod("Write", new Type[] { typeof(string) });

write?.Invoke(__0, new object[] { result });
                write?.Invoke(__0, new object[] { result });

                __result = true;
                return false;
            }
            catch (Exception e)
            {
                return true;
            }
        }
    }
}