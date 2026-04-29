using Godot;

namespace DemoMod.TheGleaner.Nodes.Vfx;

public partial class NClusterStrikeVfxOrb : Node2D {
    private AnimationPlayer _animationPlayer;

    public override void _Ready() {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void end() {
        _animationPlayer.Play("disappear");
    }
}
