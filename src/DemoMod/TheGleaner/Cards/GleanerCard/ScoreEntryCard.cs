using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace DemoMod.TheGleaner.Cards.GleanerCard;
[Pool(typeof(CardPool))]
public class ScoreEntryCard : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    public ScoreEntryCard() : base(0, CardType.Status, CardRarity.Event, TargetType.Self) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        ScorePile scorePile = ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState);
        await Cmd.Wait(0.2f);
        List<CardModel> selectedCards = (await ScorePileCmd.ShowScorePileScreen(Owner.PlayerCombatState, choiceContext, Owner)).ToList();
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
            NRun.Instance.CombatRoom.Ui.Hand.Remove(this);
        } else {
            NRun.Instance.CombatRoom.Ui.Hand.ForceRefreshCardIndices();
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
