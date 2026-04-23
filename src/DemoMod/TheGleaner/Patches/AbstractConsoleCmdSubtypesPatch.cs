using DemoMod.TheGleaner.ConsoleCommands;
using HarmonyLib;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

namespace TheGleaner.DemoMod.TheGleaner.Patches;

public class AbstractConsoleCmdSubtypesPatch {
    [HarmonyPatch(typeof(AbstractConsoleCmdSubtypes), "get_All")]
    public static class PatchAll {
        public static void Postfix(ref IReadOnlyList<Type> __result) {
            __result = [..__result, typeof(PresetDeckConsoleCmd), typeof(DumpNetCombatCardDbConsoleCmd)];
        }
    }
}
