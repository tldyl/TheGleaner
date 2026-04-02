using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Instrumentation : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    private List<IHoverTip> _hoverTips = new List<IHoverTip>();
    protected override IEnumerable<IHoverTip> ExtraHoverTips {
        get {
            if (_hoverTips.Count == 0) {
                _hoverTips.Add(HoverTipFactory.FromKeyword(CustomEnums.Concerto));
                CardModel card = ModelDb.Card<ClusterStringWeave>();
                _hoverTips.Add(HoverTipFactory.FromCard(card, true));
            }
            return _hoverTips;
        }
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    private List<CardModel> OptionCards {
        get {
            List<CardModel> ret = [];
            List<CardModel> commonCards = ModelDb.CardPool<CardPool>().AllCards.Where(c => c.Rarity == CardRarity.Common).ToList();
            List<CardModel> concertoCards = ModelDb.CardPool<CardPool>().AllCards.Where(c => c is IConcertoCard).ToList();
            CardModel commonCard = commonCards[Owner.RunState.Rng.CombatCardGeneration.NextInt(commonCards.Count)].ToMutable();
            CardModel concertoCard = concertoCards[Owner.RunState.Rng.CombatCardGeneration.NextInt(concertoCards.Count)].ToMutable();
            CardModel clusterStringWeave = ModelDb.Card<ClusterStringWeave>().ToMutable();
            commonCard.Owner = Owner;
            commonCard.UpgradeInternal();
            commonCard.FinalizeUpgradeInternal();
            concertoCard.Owner = Owner;
            concertoCard.UpgradeInternal();
            concertoCard.FinalizeUpgradeInternal();
            clusterStringWeave.Owner = Owner;
            clusterStringWeave.UpgradeInternal();
            clusterStringWeave.FinalizeUpgradeInternal();
            ret.Add(commonCard);
            ret.Add(concertoCard);
            ret.Add(clusterStringWeave);
            return ret;
        }
    }

    public Instrumentation() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        CardModel chosenCard = await CardSelectCmd.FromChooseACardScreen(choiceContext, OptionCards, Owner);
        if (chosenCard == null) {
            return;
        }
        await CardPileCmd.AddGeneratedCardToCombat(chosenCard, PileType.Hand, true);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
