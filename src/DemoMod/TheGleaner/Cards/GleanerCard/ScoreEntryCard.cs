using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class ScoreEntryCard : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Unpowered).WithMultiplier((Func<CardModel, Creature, Decimal>) ((card, _) => {
                return card.Owner.Deck.Cards.Count / 3;
            })
        )
    ];
    
    public ScoreEntryCard() : base(0, CardType.Status, CardRarity.Event, TargetType.Self) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        List<CardModel> selectedCards = (await ScorePileCmd.ShowScorePileScreen(Owner.PlayerCombatState, choiceContext, Owner)).ToList();
        ScorePile scorePile = ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState);
        if (selectedCards.Count > 0) {
            int cost = Math.Max(0, selectedCards.Count - scorePile.freeTakeCount);
            scorePile.freeTakeCount -= Math.Min(scorePile.freeTakeCount, selectedCards.Count);
            if (cost > 0) {
                await PlayerCmd.LoseEnergy(cost, Owner);
            }
            selectedCards.ForEach(card => scorePile.RemoveInternal(card));
            await CardPileCmd.Add(selectedCards, PileType.Hand);
            foreach (CardModel card in selectedCards) {
                await Hook.AfterCardChangedPiles(Owner.RunState, Owner.Creature.CombatState, card, CustomEnums.ScorePile, this);
            }
        }
        if (scorePile.Cards.Count == 0) {
            await CardPileCmd.RemoveFromCombat(this);
        }
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState) {
        if (side == CombatSide.Player) {
            ScorePile scorePile = ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState);
            scorePile.freeTakeCount = 1;
            scorePile.cardsAddedToScoreThisTurn = false;
        }
    }

    protected override PileType GetResultPileType() {
        PileType resultPileType = base.GetResultPileType();
        return resultPileType != PileType.Discard ? resultPileType : decidePile();
    }

    private PileType decidePile() {
        ScorePile scorePile = (ScorePile)CustomPiles.GetCustomPile(Owner.PlayerCombatState, CustomEnums.ScorePile);
        return scorePile.Cards.Count == 0 ? PileType.None : PileType.Hand;
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card) {
        if (card == this) {
            await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Top, skipVisuals: true);
        }
    }
}
