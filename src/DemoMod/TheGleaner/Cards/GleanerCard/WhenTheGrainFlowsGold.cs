using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;


namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class WhenTheGrainFlowsGold : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(CustomEnums.Glean)
    ];

    public WhenTheGrainFlowsGold() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        CardPile scorePile = ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState);

        if (scorePile.Cards.Count > 0) {
            CardSelectorPrefs prefs = new CardSelectorPrefs(
                new LocString("cards", "DEMOMOD-WHEN_THE_GRAIN_FLOWS_GOLD.selectionScreenPrompt"),
                scorePile.Cards.Count
            );

            IEnumerable<CardModel> selected = await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                scorePile.Cards,
                Owner,
                prefs
            );

            List<CardModel> selectedCards = selected.ToList();

            if (selectedCards.Count > 0) {
                await CardCmd.Discard(choiceContext, selectedCards);

                foreach (CardModel card in selectedCards) {
                    await Hook.AfterCardChangedPiles(
                        Owner.RunState,
                        Owner.Creature.CombatState,
                        card,
                        CustomEnums.ScorePile,
                        this
                    );
                }
            }
        }

       int capacity = ScorePileCmd.GetCapacity(Owner);

while (true) {
    CardPile currentPile = ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState);

    if (currentPile.Cards.Count >= capacity) {
        break;
    }

    await CardPileCmd.ShuffleIfNecessary(choiceContext, Owner);

    if (PileType.Draw.GetPile(Owner).Cards.Count == 0) {
        break;
    }

    await ScorePileCmd.AddCards(
        Owner.PlayerCombatState,
        Owner,
        PileType.Draw.GetPile(Owner).Cards[0]
    );
}
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}