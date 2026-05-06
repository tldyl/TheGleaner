using Godot;
using MegaCrit.Sts2.Core.Nodes;

namespace DemoMod.TheGleaner.Nodes.Vfx;

public partial class NGrayGradientVfxPostProcessor : CanvasLayer {
    public static NGrayGradientVfxPostProcessor Instance;
    public Vector2 CircleCenter { get; set; } = new Vector2(0.5f, 0.5f);
    public float Intensity { get; set; }
    public float ExpandSpeed { get; set; } = 0.8f;
    private ShaderMaterial _postProcessMaterial;
    private float _currentRadius = 0.0f;
    private bool _isExpanding = false;

    private const float MAX_RADIUS = 1.5f;

    public override void _Ready() {
        _postProcessMaterial = (ShaderMaterial)GetNode<ColorRect>("ColorRect").Material;
        _postProcessMaterial.SetShaderParameter("intensity", 0.0f);
        Instance = this;
    }

    public override void _Process(double delta) {
        if (_postProcessMaterial != null) {
            _postProcessMaterial.SetShaderParameter("intensity", Intensity);
            Vector2 screenSize = NGame.Instance.GetViewportRect().Size;
            _postProcessMaterial.SetShaderParameter("ratio", screenSize.X / screenSize.Y);
            
            if (!_isExpanding) return;
            
            _currentRadius += ExpandSpeed * (float)delta;
            if (_currentRadius > MAX_RADIUS) {
                 _currentRadius = MAX_RADIUS;
            }

            _postProcessMaterial.SetShaderParameter("radius", _currentRadius);

            if (_currentRadius >= MAX_RADIUS) {
                _isExpanding = false;
            }
        }
    }

    public void ToggleBlackAndWhite(bool enabled, float transitionDuration = 1.0f, float intensityOffset = 0.0f) {
        float targetIntensity = enabled ? 1.0f - intensityOffset : intensityOffset;

        if (transitionDuration > 0) {
            Tween tween = CreateTween().SetParallel().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Linear);
            tween.TweenProperty(this, nameof(Intensity), targetIntensity, transitionDuration);
        } else {
            Intensity = targetIntensity;
        }
    }
    
    public void TriggerExpand() {
        _currentRadius = 0.0f;
        _isExpanding = true;
    }
    
    public void TriggerExpand(Vector2 center, float speed) {
        CircleCenter = center;
        ExpandSpeed = speed;
        _postProcessMaterial.SetShaderParameter("circle_center", center);
        TriggerExpand();
    }
}
