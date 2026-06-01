using BaseLib.Abstracts;
using DemoMod.TheGleaner.Acts;
using DemoMod.TheGleaner.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Encounters;

public class FourInkcrowEncounter : CustomEncounterModel {
    public FourInkcrowEncounter() : base(RoomType.Monster) {
        
    }
    
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() {
        List<(MonsterModel, string?)> monsters = [
            (ModelDb.Monster<Inkcrow>().ToMutable(), null),
            (ModelDb.Monster<Inkcrow>().ToMutable(), null),
            (ModelDb.Monster<Inkcrow>().ToMutable(), null),
            (ModelDb.Monster<Inkcrow>().ToMutable(), null)
        ];
        ((Inkcrow)monsters[0].Item1).slotIndex = 0;
        ((Inkcrow)monsters[1].Item1).slotIndex = 1;
        ((Inkcrow)monsters[2].Item1).slotIndex = 2;
        ((Inkcrow)monsters[3].Item1).slotIndex = 3;
        return monsters;
    }

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<Inkcrow>()];
    
    public override bool IsValidForAct(ActModel act) => act is Hovercourt;
}
