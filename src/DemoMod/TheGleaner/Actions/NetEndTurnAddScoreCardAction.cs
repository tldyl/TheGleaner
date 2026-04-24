using DemoMod.TheGleaner.Cards.GleanerCard;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace DemoMod.TheGleaner.Actions;

public class NetEndTurnAddScoreCardAction : INetAction {
    public void Serialize(PacketWriter writer) {
    }

    public void Deserialize(PacketReader reader) {
    }

    public GameAction ToGameAction(Player player) {
        CardModel scoreEntryCard = ModelDb.Card<ScoreEntryCard>().ToMutable();
        scoreEntryCard.Owner = player;
        return new EndTurnAddScoreCardAction(player, scoreEntryCard);
    }
}
