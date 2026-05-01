using BaseLib.Abstracts;
using DemoMod.TheGleaner.Cards.GleanerCard;
using DemoMod.TheGleaner.Enums;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;

namespace DemoMod.TheGleaner.CardPiles;

public class ScorePile : CustomPile {
    public int freeTakeCount = 1;
    public int combatStartDeckCount = -1;
    public bool cardsAddedToScoreThisTurn = false;
    
    public ScorePile() : base(CustomEnums.ScorePile) {
    }

    public override bool CardShouldBeVisible(CardModel card) => true;

    public override Vector2 GetTargetPosition(CardModel model, Vector2 size) {
        if (NRun.Instance.CombatRoom.Ui.Hand.ActiveHolders.Any(holder => holder.CardModel is ScoreEntryCard)) {
            NHandCardHolder holder = NRun.Instance.CombatRoom.Ui.Hand.ActiveHolders.FirstOrDefault(holder => holder.CardModel is ScoreEntryCard);
            return holder.GlobalPosition;
        }
        Player player = LocalContext.GetMe(model.Owner.Creature.CombatState);
        if (player.PlayerCombatState.Hand.Cards.Count > 0) {
            return NRun.Instance.CombatRoom.Ui.Hand.GetCardHolder(player.PlayerCombatState.Hand.Cards[0]).GlobalPosition;
        }
        return NGame.Instance.GetViewportRect().Size;
    }
}
