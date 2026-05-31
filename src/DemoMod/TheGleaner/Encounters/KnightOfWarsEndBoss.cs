using BaseLib.Abstracts;
using DemoMod.TheGleaner.Acts;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using DemoMod.TheGleaner.Monsters;

namespace DemoMod.TheGleaner.Encounters;

public class KnightOfWarsEndBoss : CustomEncounterModel {
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<KnightOfWarsEnd>()];
    public override string BossNodePath => "res://TheGleaner/images/map/placeholder/knight_of_wars_end_boss_icon";

    public KnightOfWarsEndBoss() : base(RoomType.Boss) {
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() => [
        (ModelDb.Monster<KnightOfWarsEnd>().ToMutable(), null)
    ];
    
    public override bool IsValidForAct(ActModel act) => act is Hovercourt;
    
    public override float GetCameraScaling() {
        return 0.75f;
    }

    public override Vector2 GetCameraOffset() {
        return Vector2.Down * 25f;
    }
}
