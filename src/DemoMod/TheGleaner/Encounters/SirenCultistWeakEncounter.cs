using BaseLib.Abstracts;
using DemoMod.TheGleaner.Acts;
using DemoMod.TheGleaner.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Encounters;

public class SirenCultistWeakEncounter : CustomEncounterModel {
    public SirenCultistWeakEncounter() : base(RoomType.Monster) {
        
    }

    public override bool IsWeak => true;
    
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() {
        return [
            (ModelDb.Monster<SirenCultist>().ToMutable(), null)
        ];
    }
    
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<SirenCultist>()];
    
    public override bool IsValidForAct(ActModel act) => act is Hovercourt;
}
