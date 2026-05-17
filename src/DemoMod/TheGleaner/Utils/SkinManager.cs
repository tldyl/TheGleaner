using DemoMod.TheGleaner.Config;
using DemoMod.TheGleaner.Enums;

namespace DemoMod.TheGleaner.Utils;

public class SkinManager {
    public static string getVisualPath() {
        switch (GleanerModConfig.Skin) {
            case Skins.Original:
            default:
                return "res://TheGleaner/scenes/gleaner_character_original.tscn";
            case Skins.Q:
                return "res://TheGleaner/scenes/gleaner_character.tscn";
        }
    }

    public static string getRestSiteAnimPath() {
        return "res://TheGleaner/scenes/gleaner_character_rest_site.tscn";
    }

    public static string getMerchantAnimPath() {
        switch (GleanerModConfig.Skin) {
            case Skins.Original:
            default:
                return "res://TheGleaner/scenes/gleaner_character_shop_original.tscn";
            case Skins.Q:
                return "res://TheGleaner/scenes/gleaner_character_shop.tscn";
        }
    }
}
