using BaseLib.Abstracts;
using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Powers;

public class BottomlessQuiverPower : CustomPowerModel {
    public override string CustomPackedIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string CustomBigIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private int triggersUsedThisTurn;
    private bool triggeredThisTurn;

    public override CardLocation ModifyCardPlayResultLocation(
        CardModel card,
        bool isAutoPlay,
        ResourceInfo resources,
        CardLocation cardLocation) {
        if (!Owner.IsPlayer) {
            return cardLocation;
        }

        if (!card.Tags.Contains(CustomEnums.Arrow)) {
            return cardLocation;
        }
        triggersUsedThisTurn++;

        if (triggersUsedThisTurn > Amount) {
            return cardLocation;
        }
        triggeredThisTurn = true;

        Player player = Owner.Player;
        if (player == null) {
            return cardLocation;
        }
        
        if (card.Owner.Creature != Owner) {
            return cardLocation;
        }
        return cardLocation.pileType != PileType.Discard ? cardLocation : new CardLocation(Owner.Player, CustomEnums.ScorePile, CardPilePosition.Bottom);
    }
    
    public override async Task AfterModifyingCardPlayResultLocation(
        CardModel card,
        CardLocation cardLocation) {
        if (card.Owner.Creature != Owner) {
            return;
        }
        if (!Owner.IsPlayer || !triggeredThisTurn) {
            return;
        }
        Flash();
        await PlayerCmd.GainEnergy(1, Owner.Player);
    }

    public override async Task AfterCardChangedPiles(
        CardModel card,
        PileType oldPileType,
        AbstractModel? source) {
        if (triggeredThisTurn && card.Pile is ScorePile) {
            triggeredThisTurn = false;
            await ScorePileCmd.RefreshScorePileStatus(Owner.Player);
        }
    }
    
    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState) {
        if (side == Owner.Side) {
            triggersUsedThisTurn = 0;
            triggeredThisTurn = false;
        }
    }
}
