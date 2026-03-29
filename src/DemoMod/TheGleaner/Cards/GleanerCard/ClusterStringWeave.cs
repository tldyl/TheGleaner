using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class ClusterStringWeave : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    private CardModel previewCard = ModelDb.Card<ClusterStrike>().ToMutable();
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard(previewCard)];

    public ClusterStringWeave() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        List<CardModel> exhaustedCards = new List<CardModel>();
        foreach (CardModel card in PileType.Hand.GetPile(Owner).Cards.Where(c => c.Tags.Contains(CardTag.Strike) || c.Tags.Contains(CustomEnums.Arrow)).ToList()) {
            await CardCmd.Exhaust(choiceContext, card);
            exhaustedCards.Add(card);
        }
        CardPile pile = CustomPiles.GetCustomPile(Owner.PlayerCombatState, CustomEnums.ScorePile);
        if (pile != null) {
            List<CardModel> toRemove = pile.Cards.Where(c => c.Tags.Contains(CardTag.Strike) || c.Tags.Contains(CustomEnums.Arrow)).ToList();
            foreach (CardModel card in toRemove) {
                pile.RemoveInternal(card);
                await CardCmd.Exhaust(choiceContext, card);
                PileType.Exhaust.GetPile(Owner).InvokeCardAddFinished();
                exhaustedCards.Add(card);
            }
            if (pile.Cards.Count == 0 && PileType.Hand.GetPile(Owner).Cards.Any(c => c is ScoreEntryCard)) {
                CardModel scoreEntryCard = PileType.Hand.GetPile(Owner).Cards.Where(c => c is ScoreEntryCard).FirstOrDefault();
                if (scoreEntryCard != null) {
                    await CardPileCmd.RemoveFromCombat(scoreEntryCard);
                }
            }
        }
        if (exhaustedCards.Count > 0) {
            ClusterStrike clusterStrike = (ClusterStrike) ModelDb.Card<ClusterStrike>().ToMutable();
            clusterStrike.setCards(exhaustedCards);
            Owner.Creature.CombatState.AddCard(clusterStrike, Owner);
            await CardPileCmd.AddGeneratedCardToCombat(clusterStrike, PileType.Hand, true);
        }
    }

    protected override void OnUpgrade() {
        AccessTools.Method(typeof(CardModel), "OnUpgrade", []).Invoke(previewCard, []);
    }
}
