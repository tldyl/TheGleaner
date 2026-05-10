using DemoMod.TheGleaner.Powers;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace TheGleaner.DemoMod.TheGleaner.Patches;

public class NHealthBarPatch {
    [HarmonyPatch(typeof(NHealthBar), "RefreshForeground")]
    public static class PatchRefreshForeground {
        public static void Prefix(NHealthBar __instance) {
            Creature creature = AccessTools.Field(typeof(NHealthBar), "_creature").GetValue(__instance) as Creature;
            Control _poisonForeground = AccessTools.Field(typeof(NHealthBar), "_poisonForeground").GetValue(__instance) as Control;
            _poisonForeground.SelfModulate = Color.FromHtml(creature.HasPower<EtchPower>() ? "5c0026" : "79c03c");
        }
    }
}
