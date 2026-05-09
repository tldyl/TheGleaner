using BaseLib.Abstracts;
using DemoMod.TheGleaner.Monsters;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Encounters;

public class SunEncounter : CustomEncounterModel {
    public SunEncounter() : base(RoomType.Monster) {
    }

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<Sun>()
    ];

    public override bool IsValidForAct(ActModel act) {
        return act is Glory;
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() {
        return [
            (ModelDb.Monster<Sun>().ToMutable(), null)
        ];
    }

    public override float GetCameraScaling() {
        return 0.95f;
    }

    public override Vector2 GetCameraOffset() {
        return Vector2.Down * 20f;
    }
}
