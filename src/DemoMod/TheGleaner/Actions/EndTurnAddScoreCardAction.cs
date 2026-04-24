using DemoMod.TheGleaner.Commands;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Actions;

public class EndTurnAddScoreCardAction : GameAction {
    public Player Player { get; }
    private CardModel scoreEntryCard;
    public override ulong OwnerId => Player.NetId;
    public override GameActionType ActionType => GameActionType.Combat;

    public EndTurnAddScoreCardAction(Player player, CardModel card) {
        Player = player;
        scoreEntryCard = card;
    }
    
    protected override async Task ExecuteAction() {
        NetCombatCardDb.Instance.IdCardForTesting(scoreEntryCard);
        ScorePileCmd.hasScoreEntryCard.Set(Player, true);
    }

    public override INetAction ToNetAction() {
        return new NetEndTurnAddScoreCardAction();
    }
}
