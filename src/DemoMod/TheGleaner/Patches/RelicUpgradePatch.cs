using HarmonyLib;
using DemoMod.TheGleaner.Relics;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Patches;

[HarmonyPatch(typeof(MegaCrit.Sts2.Core.Models.Relics.TouchOfOrobas), "GetUpgradedStarterRelic")]
public static class TouchOfOrobasUpgradePatch
{
    [HarmonyPostfix]
    public static void Postfix(RelicModel starterRelic, ref RelicModel __result)
    {
        if (starterRelic == null)
        {
            return;
        }

        if (starterRelic.Id == ModelDb.Relic<Jera>().Id)
        {
            __result = ModelDb.Relic<ChronXIVGleaner>();
        }
    }
}