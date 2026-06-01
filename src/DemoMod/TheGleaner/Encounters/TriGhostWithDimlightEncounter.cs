using BaseLib.Abstracts;
using DemoMod.TheGleaner.Acts;
using DemoMod.TheGleaner.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Encounters;

public class TriGhostWithDimlightEncounter : CustomEncounterModel {
    public TriGhostWithDimlightEncounter() : base(RoomType.Monster) {
        
    }
    
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() {
        return [
            (ModelDb.Monster<TrighostGreen>().ToMutable(), null),
            (ModelDb.Monster<TrighostPurple>().ToMutable(), null),
            (ModelDb.Monster<Dimlight>().ToMutable(), null)
        ];
    }

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<TrighostGreen>(),
        ModelDb.Monster<TrighostPurple>(),
        ModelDb.Monster<Dimlight>()
    ];

    public override bool IsValidForAct(ActModel act) => act is Hovercourt;
}
