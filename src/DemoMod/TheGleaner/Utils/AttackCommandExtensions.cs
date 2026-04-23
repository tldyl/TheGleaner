using MegaCrit.Sts2.Core.Commands.Builders;
using TheGleaner.DemoMod.TheGleaner.Patches;

namespace DemoMod.TheGleaner.Utils;

public static class AttackCommandExtensions {
    public static AttackCommand WithHitSfxGroup(this AttackCommand command, List<string> hitSfxGroup) {
        AttackCommandPatch.HitSfxGroup.Set(command, hitSfxGroup);
        return command;
    }
}
