using BaseLib.Config;
using DemoMod.TheGleaner.Config;
using DemoMod.TheGleaner.Relics;
using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace DemoMod.TheGleaner;

[ModInitializer(nameof(initialize))]
public class TheGleanerMod {
    public static void initialize() {
        ModConfigRegistry.Register("TheGleaner", new GleanerModConfig());
        Harmony harmony = new Harmony("TheGleanerMod");
        harmony.PatchAll();

        ScriptManagerBridge.LookupScriptsInAssembly(typeof(TheGleanerMod).Assembly);
        
        CombatManager.Instance.CombatWon += onCombatWon;
    }

    public static void onCombatWon(CombatRoom room) {
        if (room.CombatState.RunState.CurrentActIndex == 0) {
            ActMap map = room.CombatState.RunState.Map;
            IReadOnlyList<MapCoord> visitedMapCoords = ((RunState)room.CombatState.RunState).VisitedMapCoords;
            MapCoord key = visitedMapCoords[^1];
            if (map.BossMapPoint.coord == key) {
                foreach (Player player in room.CombatState.RunState.Players) {
                    room.AddExtraReward(player, new RelicReward(ModelDb.Relic<HovercourtKey>().ToMutable(), player));
                }
            }
        }
    }
}

