using BaseLib.Abstracts;
using DemoMod.TheGleaner.Acts;
using DemoMod.TheGleaner.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Encounters;

public class CourtWardenEncounter : CustomEncounterModel {
    public CourtWardenEncounter() : base(RoomType.Monster) {
        
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() {
        return [
            (ModelDb.Monster<CourtWarden>().ToMutable(), null)
        ];
    }
    
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<CourtWarden>()];
    
    public override bool IsValidForAct(ActModel act) => act is Hovercourt;
}
