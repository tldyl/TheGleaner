using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Unlocks;

namespace DemoMod.TheGleaner.Acts;

public class Hovercourt : CustomActModel {
    public override IEnumerable<EventModel> AllEvents => [
        ModelDb.Event<Amalgamator>(),
        ModelDb.Event<Bugslayer>(),
        ModelDb.Event<ColorfulPhilosophers>(),
        ModelDb.Event<ColossalFlower>(),
        ModelDb.Event<FieldOfManSizedHoles>(),
        ModelDb.Event<InfestedAutomaton>(),
        ModelDb.Event<LostWisp>(),
        ModelDb.Event<SpiritGrafter>(),
        ModelDb.Event<TheLanternKey>(),
        ModelDb.Event<ZenWeaver>()
    ];

    protected override string CustomMapTopBgPath => "res://TheGleaner/images/packed/map/map_bgs/hovercourt/map_top_hovercourt.png";
    protected override string CustomMapMidBgPath => "res://TheGleaner/images/packed/map/map_bgs/hovercourt/map_middle_hovercourt.png";
    protected override string CustomMapBotBgPath => "res://TheGleaner/images/packed/map/map_bgs/hovercourt/map_bottom_hovercourt.png";
    protected override string CustomRestSiteBackgroundPath => SceneHelper.GetScenePath("rest_site/hive_rest_site");
    
    public Hovercourt() : base(2) {
        
    }

    protected override BackgroundAssets CustomGenerateBackgroundAssets(Rng rng) {
        return new CustomBackgroundAssets("res://TheGleaner/scenes/backgrounds/hovercourt/layers", "res://TheGleaner/scenes/backgrounds/hovercourt/hovercourt_background.tscn", rng);
    }
    
    public override IEnumerable<EncounterModel> GenerateAllEncounters() => [
        
    ];

    public override bool IsUnlocked(UnlockState unlockState) => true;

    public override int Index => 1;
    public override bool IsDefault => true;
}
