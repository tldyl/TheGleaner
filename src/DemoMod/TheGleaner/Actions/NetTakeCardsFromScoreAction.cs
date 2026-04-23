using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Runs;

namespace DemoMod.TheGleaner.Actions;

public class NetTakeCardsFromScoreAction : INetAction {
    public ulong netId;
    
    public void Serialize(PacketWriter writer) {
        writer.WriteULong(netId);
    }

    public void Deserialize(PacketReader reader) {
        netId = reader.ReadULong();
    }

    public GameAction ToGameAction(Player player) {
        return new TakeCardsFromScoreAction(RunManager.Instance.DebugOnlyGetState().GetPlayer(netId));
    }
}
