using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Cards.TokenCards;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;


namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Improvisation : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(3)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Score)];
    
    private List<CardModel> optionCards;
    private List<CardModel> OptionCards {
        get {
            if (optionCards == null) {
                optionCards = [
                    ModelDb.Card<ZeroCostAttacks>().ToMutable(),
                    ModelDb.Card<OneCostAttacks>().ToMutable(),
                    ModelDb.Card<TwoCostAttacks>().ToMutable(),
                    ModelDb.Card<ThreeOrMoreCostAttacks>().ToMutable()
                ];
                foreach (CardModel card in optionCards) {
                    card.Owner = Owner;
                    ((IChoosable) card).addVar(DynamicVars.Cards);
                }
            }
            return optionCards;
        }
    }

    public Improvisation() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        CardModel chosenCard = await CardSelectCmd.FromChooseACardScreen(choiceContext, OptionCards, Owner);
        if (chosenCard != null) {
            await ((IChoosable) chosenCard).OnChosen(choiceContext, cardPlay);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}