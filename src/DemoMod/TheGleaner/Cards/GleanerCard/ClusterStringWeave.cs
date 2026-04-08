using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class ClusterStringWeave : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    private CardModel previewCard = ModelDb.Card<ClusterStrike>().ToMutable();
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard(previewCard)];

    public ClusterStringWeave() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        List<CardModel> mergedCards = PileType.Hand.GetPile(Owner).Cards
            .Where(c => c.Tags.Contains(CardTag.Strike) || c.Tags.Contains(CustomEnums.Arrow))
            .ToList();
        CardPile pile = ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState);
        if (pile != null) {
            List<CardModel> toRemove = pile.Cards.Where(c => c.Tags.Contains(CardTag.Strike) || c.Tags.Contains(CustomEnums.Arrow)).ToList();
            foreach (CardModel card in toRemove) {
                mergedCards.Add(card);
            }
        }
        if (mergedCards.Count > 1) {
            foreach (CardModel card in mergedCards) {
                await CardPileCmd.RemoveFromCombat(card);
            }
            if (pile.Cards.Count == 0 && NRun.Instance.CombatRoom.Ui.Hand.ActiveHolders.Any(holder => holder.CardModel is ScoreEntryCard)) {
                NRun.Instance.CombatRoom.Ui.Hand.RemoveCardHolder(NRun.Instance.CombatRoom.Ui.Hand.ActiveHolders.FirstOrDefault(holder => holder.CardModel is ScoreEntryCard));
            }
            ClusterStrike clusterStrike = (ClusterStrike) ModelDb.Card<ClusterStrike>().ToMutable();
            if (CurrentUpgradeLevel > 0) {
                clusterStrike.UpgradeInternal();
                clusterStrike.FinalizeUpgradeInternal();
            }
            clusterStrike.setCards(mergedCards);
            Owner.Creature.CombatState.AddCard(clusterStrike, Owner);
            await CardPileCmd.AddGeneratedCardToCombat(clusterStrike, CustomEnums.ScorePile, true);
        }
    }

    protected override void OnUpgrade() {
        previewCard.DowngradeInternal();
        AccessTools.Method(typeof(CardModel), "OnUpgrade", []).Invoke(previewCard, []);
        previewCard.FinalizeUpgradeInternal();
    }
}
