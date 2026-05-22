using BaseLib.Abstracts;
using DemoMod.TheGleaner.Monsters;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Encounters;

public class KingAndPawnsEncounter : CustomEncounterModel {
    public KingAndPawnsEncounter() : base(RoomType.Monster) {
    }

    public override bool IsWeak => true;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<King>(),
        ModelDb.Monster<Pawn>()
    ];

    public override bool IsValidForAct(ActModel act) {
        return act is Glory;
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() {
        return [
            (CreateMinionPawn(), null),
            (ModelDb.Monster<King>().ToMutable(), null),
            (CreateMinionPawn(), null)
        ];
    }

    private static Pawn CreateMinionPawn() {
        Pawn pawn = (Pawn)ModelDb.Monster<Pawn>().ToMutable();
        pawn.StartsWithMinionPower = true;
        return pawn;
    }

    public override float GetCameraScaling() {
        return 0.95f;
    }

    public override Vector2 GetCameraOffset() {
        return Vector2.Down * 30f;
    }
}
