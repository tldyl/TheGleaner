using BaseLib.Abstracts;
using DemoMod.TheGleaner.Acts;
using DemoMod.TheGleaner.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Encounters;

public class ZapfliSorceressEncounter : CustomEncounterModel {
    public override string CustomScenePath => "res://TheGleaner/scenes/encounters/zapfli_sorceress.tscn";
    
    public ZapfliSorceressEncounter() : base(RoomType.Monster) {
        
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() {
        return [
            (ModelDb.Monster<ZapfliSorceress>().ToMutable(), "Slot0"),
            (ModelDb.Monster<Zapfli>().ToMutable(), "Slot1"),
            (ModelDb.Monster<Zapfli>().ToMutable(), "Slot2")
        ];
    }
    
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<ZapfliSorceress>(),
        ModelDb.Monster<Zapfli>()
    ];

    public override bool IsValidForAct(ActModel act) => act is Hovercourt;
}
