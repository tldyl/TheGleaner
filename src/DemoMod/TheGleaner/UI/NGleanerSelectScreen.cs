using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace TheGleaner.DemoMod.TheGleaner.UI;

public partial class NGleanerSelectScreen : Control {
    private Node2D spineNode;
    
    public override void _Ready() {
        spineNode = GetNode<Node2D>("SpineSprite");
        MegaSprite sprite = new MegaSprite(spineNode);
        MegaTrackEntry track = sprite.GetAnimationState().SetAnimation("animation");
    }
}
