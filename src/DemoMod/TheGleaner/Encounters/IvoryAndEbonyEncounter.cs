using BaseLib.Abstracts;
using DemoMod.TheGleaner.Monsters;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Encounters;

public class IvoryAndEbonyEncounter : CustomEncounterModel {
    public IvoryAndEbonyEncounter() : base(RoomType.Elite) {
        
    }
    
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<IvoryBishop>(),
        ModelDb.Monster<EbonyBishop>()
    ];
    
    public override bool IsValidForAct(ActModel act) {
        return act is Glory;
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() {
        return [
            (ModelDb.Monster<IvoryBishop>().ToMutable(), null),
            (ModelDb.Monster<EbonyBishop>().ToMutable(), null)
        ];
    }

    public override float GetCameraScaling() {
        return 0.95f;
    }

    public override Vector2 GetCameraOffset() {
        return Vector2.Down * 25f;
    }
}
