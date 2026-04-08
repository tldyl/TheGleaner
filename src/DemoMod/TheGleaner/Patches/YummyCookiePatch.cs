using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;

namespace TheGleaner.DemoMod.TheGleaner.Patches;

public class YummyCookiePatch {
    [HarmonyPatch(typeof(YummyCookie), "get_IconBaseName")]
    public static class PatchIconBaseName {
        public static void Postfix(YummyCookie __instance, ref string __result) {
            CharacterModel characterModel;
            if (__instance.IsCanonical || __instance.Owner == null) {
                CharacterModel _cachedRandomCharacter = (CharacterModel) AccessTools.Field(typeof(YummyCookie), "_cachedRandomCharacter").GetValue(null);
                if (_cachedRandomCharacter == null) {
                    _cachedRandomCharacter = Rng.Chaotic.NextItem(ModelDb.AllCharacters);
                    AccessTools.Field(typeof(YummyCookie), "_cachedRandomCharacter").SetValue(null, _cachedRandomCharacter);
                }
                characterModel = _cachedRandomCharacter;
            } else {
                characterModel = __instance.Owner.Character;
            }
            if (characterModel is global::DemoMod.TheGleaner.Characters.TheGleaner) {
                __result = "yummy_cookie_gleaner";
            }
        }
    }
}
