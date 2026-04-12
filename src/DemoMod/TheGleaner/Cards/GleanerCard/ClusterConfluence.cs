using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class ClusterConfluence : CustomCardModel {
    //public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<StrikeGleaner>(), HoverTipFactory.FromCard<ClusterStrike>()];
    protected override bool HasEnergyCostX => true;

    public ClusterConfluence() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        for (int _ = 0; _ < ResolveEnergyXValue() + CurrentUpgradeLevel; _++) {
            CardModel strike = ModelDb.Card<StrikeGleaner>().ToMutable();
            Owner.Creature.CombatState.AddCard(strike, Owner);
            await CardPileCmd.AddGeneratedCardToCombat(strike, PileType.Hand, true);
        }
        List<CardModel> mergedCards = PileType.Hand.GetPile(Owner).Cards
            .Where(c => c.Tags.Contains(CardTag.Strike) || c.Tags.Contains(CustomEnums.Arrow))
            .ToList();

        CardPile pile = ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState);
        if (pile != null) {
            List<CardModel> toRemove = pile.Cards
                .Where(c => c.Tags.Contains(CardTag.Strike) || c.Tags.Contains(CustomEnums.Arrow))
                .ToList();

            foreach (CardModel card in toRemove) {
                mergedCards.Add(card);
            }
        }

        if (mergedCards.Count > 1) {
            foreach (CardModel card in mergedCards) {
                await CardPileCmd.RemoveFromCombat(card);
            }

            ClusterStrike clusterStrike = (ClusterStrike)ModelDb.Card<ClusterStrike>().ToMutable();
            if (CurrentUpgradeLevel > 0) {
                clusterStrike.UpgradeInternal();
                clusterStrike.FinalizeUpgradeInternal();
            }

            clusterStrike.setCards(mergedCards);
            Owner.Creature.CombatState.AddCard(clusterStrike, Owner);
            await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, clusterStrike);
        }
    }
}
