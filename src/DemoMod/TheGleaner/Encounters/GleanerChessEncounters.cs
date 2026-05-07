using BaseLib.Abstracts;
using DemoMod.TheGleaner.Monsters;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Encounters;

public sealed class GleanerCastleAndPawnsEncounter : CustomEncounterModel
{
	public GleanerCastleAndPawnsEncounter() : base(RoomType.Monster, false)
	{
	}

	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
	[
		ModelDb.Monster<GleanerCastle>(),
		ModelDb.Monster<GleanerPawn>()
	];

	public override bool IsValidForAct(ActModel act)
	{
		return act is Glory;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return
		[
			(ModelDb.Monster<GleanerPawn>().ToMutable(), null),
			(ModelDb.Monster<GleanerCastle>().ToMutable(), null),
			(ModelDb.Monster<GleanerPawn>().ToMutable(), null)
		];
	}

	public override float GetCameraScaling()
	{
		return 0.95f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 30f;
	}
}

public sealed class GleanerIvoryAndEbonyEncounter : CustomEncounterModel
{
	public GleanerIvoryAndEbonyEncounter() : base(RoomType.Elite, false)
	{
	}

	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
	[
		ModelDb.Monster<GleanerIvoryBishop>(),
		ModelDb.Monster<GleanerEbonyBishop>()
	];

	public override bool IsValidForAct(ActModel act)
	{
		return act is Glory;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return
		[
			(ModelDb.Monster<GleanerIvoryBishop>().ToMutable(), null),
			(ModelDb.Monster<GleanerEbonyBishop>().ToMutable(), null)
		];
	}

	public override float GetCameraScaling()
	{
		return 0.95f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 25f;
	}
}
