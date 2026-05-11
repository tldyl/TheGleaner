using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;
using DemoMod.TheGleaner.Monsters;

namespace DemoMod.TheGleaner.Encounters;

public class KnightOfWarsEndBoss : CustomEncounterModel {
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<KnightOfWarsEnd>()];
    public override string BossNodePath {
        get => "res://images/map/placeholder/test_subject_boss_icon.png";
    }
    
    public KnightOfWarsEndBoss() : base(RoomType.Boss) {
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() => [
        (ModelDb.Monster<KnightOfWarsEnd>().ToMutable(), null)
    ];
    
    public override bool IsValidForAct(ActModel act) => act is Glory;
    
    public override float GetCameraScaling() {
        return 0.95f;
    }

    public override Vector2 GetCameraOffset() {
        return Vector2.Down * 25f;
    }
}
