using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Patches;

[HarmonyPatch(typeof(ModelDb), "get_AllCharacters")]
public class AddCharacterPatch {
    public static void Postfix(ref IEnumerable<CharacterModel> __result) {
        // __result = [
        //     .. __result, .. new CharacterModel[]
        //     {
        //         ModelDb.Character<DemoMod.TheGleaner.Characters.TheGleaner>()
        //     }
        // ];
    }
}
