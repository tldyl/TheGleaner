using BaseLib.Abstracts;
using DemoMod.TheGleaner.Acts;
using DemoMod.TheGleaner.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Encounters;

public class SnakePlantMatriarchElite : CustomEncounterModel {
    public SnakePlantMatriarchElite() : base(RoomType.Elite) {
        
    }
    
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() {
        return [
            (ModelDb.Monster<SnakePlantMatriarch>().ToMutable(), null)
        ];
    }
    
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<SnakePlantMatriarch>()];
    
    public override bool IsValidForAct(ActModel act) => act is Hovercourt;
}
