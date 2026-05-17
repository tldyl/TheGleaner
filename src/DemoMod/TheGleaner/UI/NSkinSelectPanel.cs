using DemoMod.TheGleaner.Config;
using DemoMod.TheGleaner.Enums;
using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace TheGleaner.DemoMod.TheGleaner.UI;

public partial class NSkinSelectPanel : VBoxContainer {
    private NButton _leftArrow;
    private NButton _rightArrow;
    private Label _skinLabel;
    private Label _skinNameLabel;
    
    public override void _Ready() {
        _leftArrow = GetNode<NButton>((NodePath) "HBoxContainer/LeftArrow");
        _rightArrow = GetNode<NButton>((NodePath) "HBoxContainer/RightArrow");
        _leftArrow.Connect(NClickableControl.SignalName.Released, Callable.From((Action<NButton>) (_ => SelectPrevSkin())));
        _rightArrow.Connect(NClickableControl.SignalName.Released, Callable.From((Action<NButton>) (_ => SelectNextSkin())));
        _skinLabel = GetNode<Label>((NodePath) "SkinLabel");
        _skinNameLabel = GetNode<Label>((NodePath) "HBoxContainer/SkinNameLabel");
        _skinLabel.Text = new LocString("settings_ui", "DEMOMOD-SKIN.title").GetFormattedText();
        UpdateSkinName();
    }

    private void SelectPrevSkin() {
        Skins skin = GleanerModConfig.Skin;
        Skins[] skins = Enum.GetValues<Skins>();
        int index = Array.IndexOf(skins, skin);
        GleanerModConfig.Skin = index == 0 ? skins[skins.Length - 1] : skins[index - 1];
        UpdateSkinName();
    }
    
    private void SelectNextSkin() {
        Skins skin = GleanerModConfig.Skin;
        Skins[] skins = Enum.GetValues<Skins>();
        int index = Array.IndexOf(skins, skin);
        GleanerModConfig.Skin = index == skins.Length - 1 ? skins[0] : skins[index + 1];
        UpdateSkinName();
    }

    private void UpdateSkinName() {
        _skinNameLabel.Text = new LocString("settings_ui", $"DEMOMOD-SKIN.{Enum.GetName(GleanerModConfig.Skin)}.title").GetFormattedText();
    }
}
