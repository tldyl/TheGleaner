using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;

namespace DemoMod.TheGleaner.Nodes.Vfx;

public partial class NAoeAttackVfx : Node2D {
    private CancellationTokenSource? _cts;
    private GpuParticles2D _particles;
    
    public override void _Ready() {
        _particles = GetNode<GpuParticles2D>("vfx_slash_core");
        TaskHelper.RunSafely(PlaySequence());
    }

    private async Task PlaySequence() {
        _cts = new CancellationTokenSource();
        _particles.Restart();
        await Cmd.Wait(1f, _cts.Token);
        this.QueueFreeSafely();
    }
}
