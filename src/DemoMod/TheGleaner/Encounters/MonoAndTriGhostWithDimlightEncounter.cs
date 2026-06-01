using BaseLib.Abstracts;
using DemoMod.TheGleaner.Acts;
using DemoMod.TheGleaner.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Encounters;

public class MonoAndTriGhostWithDimlightEncounter : CustomEncounterModel {
    public MonoAndTriGhostWithDimlightEncounter() : base(RoomType.Monster) {
        
    }
    
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() {
        return [
            (ModelDb.Monster<MonoghostRed>().ToMutable(), null),
            (ModelDb.Monster<MonoghostBlue>().ToMutable(), null),
            (((MonsterModel) (Rng.NextBool() ? ModelDb.Monster<TrighostGreen>() : ModelDb.Monster<TrighostPurple>())).ToMutable(), null),
            (ModelDb.Monster<Dimlight>().ToMutable(), null)
        ];
    }

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<MonoghostRed>(),
        ModelDb.Monster<MonoghostBlue>(),
        ModelDb.Monster<TrighostGreen>(),
        ModelDb.Monster<TrighostPurple>(),
        ModelDb.Monster<Dimlight>()
    ];

    public override bool IsValidForAct(ActModel act) => act is Hovercourt;
}
