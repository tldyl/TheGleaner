using BaseLib.Abstracts;
using DemoMod.TheGleaner.Acts;
using DemoMod.TheGleaner.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Encounters;

public class SlimeGuardianWeakEncounter : CustomEncounterModel {
    public SlimeGuardianWeakEncounter() : base(RoomType.Monster) {
        
    }
    
    public override bool IsWeak => true;
    public override string CustomScenePath => "res://TheGleaner/scenes/encounters/slime_guardian_weak.tscn";
    
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() {
        return [
            (ModelDb.Monster<SlimeGuardian>().ToMutable(), "Slot0")
        ];
    }
    
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<SlimeGuardian>()];
    
    public override bool IsValidForAct(ActModel act) => act is Hovercourt;
}
