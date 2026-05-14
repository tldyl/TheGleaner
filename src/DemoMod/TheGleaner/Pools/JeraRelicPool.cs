using System.Collections.Generic;
using DemoMod.TheGleaner.Relics;
using Godot;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Pools;

public class JeraRelicPool : RelicPoolModel {
	public override string EnergyColorName => "gleaner";
	public override Color LabOutlineColor => Colors.White;

	protected override IEnumerable<RelicModel> GenerateAllRelics() {
		return new List<RelicModel> {
			ModelDb.Relic<Jera>(),
			ModelDb.Relic<TriangleHairClip>(),
			ModelDb.Relic<GreenAppleScentedVessel>(),

		};
	}
}
