using Godot;
using MegaCrit.Sts2.Core.Helpers;

namespace DemoMod.TheGleaner.Nodes.Vfx;

public partial class NOpticalFlareVfx : Node2D {
    private float blinkSpeed = 0.05f;
    private double timer;
    private double fadeAlpha;
    private double fadeDirection = 1.0;

    public override void _Process(double delta) {
        timer += delta;
        fadeAlpha += delta * 2.0 * fadeDirection;
        if (fadeAlpha > 1) {
            fadeAlpha = 1;
        }
        if (fadeAlpha < 0) {
            fadeAlpha = 0;
        }
        if (fadeDirection < 0 && fadeAlpha <= 0) {
            this.QueueFreeSafely();
        }
        if (timer >= blinkSpeed) {
            timer = 0;
            foreach (Node node in GetChildren()) {
                if (node is not Control child) {
                    continue;
                }
                double randomAlpha = GD.RandRange(0.2f, 1.0f) * fadeAlpha;
                Material material = child.Material;
                switch (material) {
                    case ShaderMaterial shaderMaterial:
                        shaderMaterial.SetShaderParameter("alpha_blink", (float) randomAlpha);
                        break;
                    case CanvasItemMaterial:
                        child.Modulate = new Color(child.Modulate.R, child.Modulate.G, child.Modulate.B, (float) randomAlpha);
                        break;
                }
            }
        }
    }

    public void End() {
        fadeDirection = -1.0;
    }
}
