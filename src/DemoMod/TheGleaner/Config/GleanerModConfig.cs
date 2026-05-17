using BaseLib.Config;
using DemoMod.TheGleaner.Enums;

namespace DemoMod.TheGleaner.Config;

public class GleanerModConfig : SimpleModConfig {
    [ConfigHideInUI]
    public static Skins Skin { get; set; } = Skins.Original;
}
