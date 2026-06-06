using BaseLib.Abstracts;
using DemoMod.TheGleaner.Acts;
using DemoMod.TheGleaner.Monsters;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Encounters;

public class PortalsBoss : CustomEncounterModel {
    public override string BossNodePath => "res://TheGleaner/images/map/placeholder/knight_of_wars_end_boss_icon";
    
    public PortalsBoss() :base(RoomType.Boss) {
        
    }
    
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<ScrutinyDoor>(),
        ModelDb.Monster<HungerDoor>(),
        ModelDb.Monster<GraspDoor>()
    ];
    
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() => [
        (ModelDb.Monster<ScrutinyDoor>().ToMutable(), null),
        (ModelDb.Monster<HungerDoor>().ToMutable(), null),
        (ModelDb.Monster<GraspDoor>().ToMutable(), null)
    ];
    
    public override bool IsValidForAct(ActModel act) => act is Hovercourt;
    
    public override float GetCameraScaling() {
        return 0.75f;
    }

    public override Vector2 GetCameraOffset() {
        return Vector2.Down * 25f;
    }
}
