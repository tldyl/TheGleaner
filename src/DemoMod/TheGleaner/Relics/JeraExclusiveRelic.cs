using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using JeraCharacter = DemoMod.TheGleaner.Characters.TheGleaner;

namespace DemoMod.TheGleaner.Relics;

public abstract class JeraExclusiveRelic : CustomRelicModel {
    public override bool IsAllowed(IRunState runState) {
        if (runState?.Players == null) {
            return false;
        }

        return runState.Players.Any(IsJeraPlayer);
    }

    protected bool IsOwnerJera() => IsJeraPlayer(Owner);

    protected static bool IsJeraPlayer(Player? player) {
        if (player?.Character == null) {
            return false;
        }

        if (player.Character is JeraCharacter) {
            return true;
        }

        return player.Character.Id == ModelDb.Character<JeraCharacter>().Id;
    }
}
