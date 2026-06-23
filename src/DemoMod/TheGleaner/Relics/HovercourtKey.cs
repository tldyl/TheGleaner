using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Acts;
using DemoMod.TheGleaner.Pools;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace DemoMod.TheGleaner.Relics;

[Pool(typeof(JeraRelicPool))]
public class HovercourtKey : CustomRelicModel {
    public override RelicRarity Rarity => RelicRarity.Event;

    public override async Task AfterObtained() {
        Flash();
        int _currentActIndex = (int) AccessTools.Field(typeof(RunState), "_currentActIndex").GetValue(Owner.RunState);
        _currentActIndex++;
        AccessTools.Field(typeof(RunState), "_currentActIndex").SetValue(Owner.RunState, _currentActIndex);
        ActModel act = ModelDb.Act<Hovercourt>().ToMutable();
        act.GenerateRooms(Owner.RunState.Rng.UpFront, Owner.RunState.UnlockState, Owner.RunState.Players.Count > 1);
        ((RunState)Owner.RunState).SetActDebug(act);
        _currentActIndex--;
        AccessTools.Field(typeof(RunState), "_currentActIndex").SetValue(Owner.RunState, _currentActIndex);
    }
}
