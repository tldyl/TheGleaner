using BaseLib.Config;
using DemoMod.TheGleaner.Config;
using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace DemoMod.TheGleaner;

[ModInitializer(nameof(initialize))]
public class TheGleanerMod {
    public static void initialize() {
        ModConfigRegistry.Register("TheGleaner", new GleanerModConfig());
        Harmony harmony = new Harmony("TheGleanerMod");
        harmony.PatchAll();

        ScriptManagerBridge.LookupScriptsInAssembly(typeof(TheGleanerMod).Assembly);
    }
}

