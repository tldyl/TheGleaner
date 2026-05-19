using BaseLib.Abstracts;
using DemoMod.TheGleaner.Utils;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Runs;

namespace DemoMod.TheGleaner.Messages;

public class CardTransformMessage : ICustomMessage {
    public int transformAmount;
    
    public void Serialize(PacketWriter writer) {
        writer.WriteInt(transformAmount);
    }

    public void Deserialize(PacketReader reader) {
        transformAmount = reader.ReadInt();
    }

    public void HandleMessage(ulong senderId) {
        Player player = RunManager.Instance.DebugOnlyGetState().GetPlayer(senderId);
        if (LocalContext.IsMe(player)) {
            throw new InvalidOperationException("CardTransformMessage should not be sent to the player transforming the card!");
        }
        TaskHelper.RunSafely(RunManager.Instance.RewardSynchronizer.DoCardTransform(player, transformAmount));
    }

    public bool ShouldBroadcast { get; }
}
