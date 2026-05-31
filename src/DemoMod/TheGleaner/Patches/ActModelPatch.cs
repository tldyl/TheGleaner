using DemoMod.TheGleaner.Acts;
using DemoMod.TheGleaner.Config;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Unlocks;

namespace TheGleaner.DemoMod.TheGleaner.Patches;

public class ActModelPatch {
    public static class PatchGetRandomList {
        public static void Postfix(Rng rng, UnlockState unlockState, bool isMultiplayer, ref IEnumerable<ActModel> __result) {
            List<ActModel> list = __result.ToList();
            if (GleanerModConfig.AlwaysHovercourt) {
                list[1] = ModelDb.Act<Hovercourt>();
            } else if (rng.NextBool()) {
                list[1] = ModelDb.Act<Hovercourt>();
            }
            __result = list;
        }
    }
}
