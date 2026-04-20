using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class ReclaimingTheStray : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Score)];

    public ReclaimingTheStray() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
    }

    public override async Task BeforeCombatStart() {
        if (!IsInCombat || CombatState == null || Owner.Deck.Cards.Contains(this)) {
            return;
        }

        if (IsUpgraded) {
            CardModel cpy = CreateClone();
            await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, cpy);
        }
        
        await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        CardPile discardPile = PileType.Discard.GetPile(Owner);
        if (discardPile.Cards.Count == 0) {
            return;
        }

        CardSelectorPrefs prefs = new CardSelectorPrefs(
            new LocString("cards", "DEMOMOD-RECLAIMING_THE_STRAY.selectionScreenPrompt"),
            DynamicVars.Cards.IntValue
        );

        IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            discardPile.Cards,
            Owner,
            prefs
        );

        foreach (CardModel selectedCard in selectedCards) {
            if (selectedCard != null) {
                await CardPileCmd.Add(selectedCard, PileType.Hand, CardPilePosition.Bottom, null, false);
            }
        }
    }
}