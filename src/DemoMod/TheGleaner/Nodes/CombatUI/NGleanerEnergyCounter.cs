using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace DemoMod.TheGleaner.Nodes;

public partial class NGleanerEnergyCounter : NEnergyCounter {
    public override void _Ready() {
        base._Ready();
        MegaLabel _label = GetNode<MegaLabel>("Label");
        _label.Position = new Vector2(40, 40);
        Font font = (Font)ResourceLoader.Load("res://fonts/kreon_bold.ttf", "Font");
        _label.AddThemeFontOverride("font", font);
        _label.AddThemeFontSizeOverride("font_size", 36);
        _label.AddThemeColorOverride("font_color", Color.FromHtml("fff6f4"));
        _label.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.1882353f));
        _label.AddThemeColorOverride("font_outline_color", new Color(0.7294118f, 0.5647059f, 0.24313726f, 1));
        _label.AddThemeConstantOverride("shadow_offset_x", 3);
        _label.AddThemeConstantOverride("shadow_offset_y", 2);
        _label.AddThemeConstantOverride("outline_size", 16);
        _label.AddThemeConstantOverride("shadow_outline_size", 16);
    }
}