using Godot;
using Godot.Collections;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace DemoMod.TheGleaner.Nodes;

public partial class NGleanerParticlesContainer : NParticlesContainer {
    public override void _Ready() {
        AccessTools.Field(typeof(NParticlesContainer), "_particles").SetValue(this, new Array<GpuParticles2D>());
    }
}
