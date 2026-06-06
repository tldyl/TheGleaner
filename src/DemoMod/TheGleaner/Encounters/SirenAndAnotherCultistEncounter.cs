using BaseLib.Abstracts;
using DemoMod.TheGleaner.Acts;
using DemoMod.TheGleaner.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Encounters;

public class SirenAndAnotherCultistEncounter : CustomEncounterModel {
    public SirenAndAnotherCultistEncounter() : base(RoomType.Monster) {
        
    }
    
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() {
        return [
            (ModelDb.Monster<SirenCultist>().ToMutable(), null),
            (((MonsterModel) (Rng.NextBool() ? ModelDb.Monster<CalcifiedCultist>() : ModelDb.Monster<DampCultist>())).ToMutable(), null)
        ];
    }
    
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<SirenCultist>(),
        ModelDb.Monster<CalcifiedCultist>(),
        ModelDb.Monster<DampCultist>()
    ];
    
    public override bool IsValidForAct(ActModel act) => act is Hovercourt;
}
