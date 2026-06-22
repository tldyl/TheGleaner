using BaseLib.Abstracts;
using DemoMod.TheGleaner.Encounters;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Events;

public class TestCombatEvent : CustomEventModel {
    public override EventLayoutType LayoutType => EventLayoutType.Default;
    public override EncounterModel CanonicalEncounter => ModelDb.Encounter<FourInkcrowEncounter>();
    public override bool IsShared => true;
    public override string CustomInitialPortraitPath => "res://TheGleaner/images/events/demomod-test_combat_event.png";
    
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() {
        return [
            new EventOption(this, Fight, "DEMOMOD-TEST_COMBAT_EVENT.pages.INITIAL.options.FIGHT"),
            new EventOption(this, Leave, "DEMOMOD-TEST_COMBAT_EVENT.pages.INITIAL.options.LEAVE")
        ];
    }

    private async Task Fight() {
        SetEventState(L10NLookup("DEMOMOD-TEST_COMBAT_EVENT.pages.FIGHT.description"), [
            new EventOption(this, () => {
                EnterCombatWithoutExitingEvent<FourInkcrowEncounter>([], true);
                return Task.CompletedTask;
            }, "DEMOMOD-TEST_COMBAT_EVENT.pages.FIGHT.options.FIGHT")
        ]);
    }

    private async Task Leave() {
        SetEventFinished(L10NLookup("DEMOMOD-TEST_COMBAT_EVENT.pages.LEAVE.description"));
    }
    
    public override async Task Resume(AbstractRoom room) {
        SetEventFinished(L10NLookup("DEMOMOD-TEST_COMBAT_EVENT.pages.RESUME.description"));
    }
}
