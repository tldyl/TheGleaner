using BaseLib.Abstracts;
using DemoMod.TheGleaner.Enums;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;

namespace DemoMod.TheGleaner.CardPiles;

public class ScorePile : CustomPile {
    public int freeTakeCount = 1;
    public int combatStartDeckCount = -1;
    
    public ScorePile() : base(CustomEnums.ScorePile) {
    }

    public override bool CardShouldBeVisible(CardModel card) => true;

    public override Vector2 GetTargetPosition(CardModel model, Vector2 size) => NGame.Instance.GetViewportRect().Size;
}
