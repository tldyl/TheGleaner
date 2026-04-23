using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Actions;

public class TakeCardsFromScoreAction : GameAction {
    public Player Player { get; }
    public override ulong OwnerId => Player.NetId;
    public override GameActionType ActionType => GameActionType.CombatPlayPhaseOnly;

    public TakeCardsFromScoreAction(Player player) {
        Player = player;
    }
    
    protected override async Task ExecuteAction() {
        ScorePile scorePile = ScorePileCmd.GetOrCreateScorePile(Player.PlayerCombatState);
        await Cmd.Wait(0.2f);
        List<CardModel> selectedCards = (await ScorePileCmd.ShowScorePileScreen(Player.PlayerCombatState, new GameActionPlayerChoiceContext(this), Player)).ToList();
        if (selectedCards.Count > 0) {
            int cost = Math.Max(0, selectedCards.Count - scorePile.freeTakeCount);
            scorePile.freeTakeCount -= Math.Min(scorePile.freeTakeCount, selectedCards.Count);
            if (cost > 0) {
                await PlayerCmd.LoseEnergy(cost, Player);
            }
            selectedCards.ForEach(card => scorePile.RemoveInternal(card));
            await CardPileCmd.Add(selectedCards, PileType.Hand);
            foreach (CardModel card in selectedCards) {
                await Hook.AfterCardChangedPiles(Player.RunState, Player.Creature.CombatState, card, CustomEnums.ScorePile, null);
            }
        }
        GleanerVfxCmd.CheckScoreIsEmpty(Player.PlayerCombatState);
    }

    public override INetAction ToNetAction() {
        return new NetTakeCardsFromScoreAction {
            netId = Player.NetId
        };
    }
}
