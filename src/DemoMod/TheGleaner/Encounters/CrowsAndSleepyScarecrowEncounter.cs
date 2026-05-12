using BaseLib.Abstracts;
using DemoMod.TheGleaner.Monsters;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Encounters;

public class CrowsAndSleepyScarecrowEncounter : CustomEncounterModel {
    public CrowsAndSleepyScarecrowEncounter() : base(RoomType.Monster) {
    }

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<Crow>(),
        ModelDb.Monster<SleepyScarecrow>()
    ];

    public override bool IsValidForAct(ActModel act) {
        return act is Glory;
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() {
        return [
            (CreateCrow(0), null),
            (CreateCrow(1), null),
            (ModelDb.Monster<SleepyScarecrow>().ToMutable(), null),
            (CreateCrow(2), null)
        ];
    }

    private static Crow CreateCrow(int coordinatedActionSlot) {
        Crow crow = (Crow)ModelDb.Monster<Crow>().ToMutable();
        crow.CoordinatedActionSlot = coordinatedActionSlot;
        return crow;
    }

    public override float GetCameraScaling() {
        return 0.9f;
    }

    public override Vector2 GetCameraOffset() {
        return Vector2.Down * 25f;
    }
}
