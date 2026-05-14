using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace DemoMod.TheGleaner.Nodes.Vfx;

public partial class NPrismaticRainfallStrikeVfx : Node2D {
	private ShaderMaterial _material;

	public static NPrismaticRainfallStrikeVfx Create(Creature target) {
		NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(target);
		Vector2 targetPosition = creatureNode.Visuals.GetNode<Marker2D>("%CenterPos").GlobalPosition;
		NPrismaticRainfallStrikeVfx vfx = PreloadManager.Cache.GetScene("res://TheGleaner/scenes/vfx/prismatic_rainfall_strike_vfx.tscn").Instantiate<NPrismaticRainfallStrikeVfx>();
		Line2D line2D = vfx.GetNode<Line2D>("Line2D");
		line2D.Points = [line2D.Points[0], targetPosition];
		ShaderMaterial material = line2D.Material as ShaderMaterial;
		GradientTexture1D lut = (GradientTexture1D) material.GetShaderParameter("lut");
		lut.Gradient.Colors = [
			new Color((float) GD.RandRange(0.1f, 0.3f), (float) GD.RandRange(0.15f, 0.3f), (float) GD.RandRange(0.2f, 0.3f)),
			new Color((float) GD.RandRange(0.5f, 1.0f), (float) GD.RandRange(0.3f, 0.8f), (float) GD.RandRange(0.1f, 0.6f))
		];
		return vfx;
	}
	
	public override void _Ready() {
		_material = (ShaderMaterial) GetNode<Line2D>("Line2D").Material;
	}
	
	public void setVfxAlpha(float alpha) {
		_material.SetShaderParameter("blink_alpha", alpha);
	}
}
