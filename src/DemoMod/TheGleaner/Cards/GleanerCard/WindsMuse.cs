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
public class WindsMuse : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Amount", 3)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Glean)];

    public WindsMuse() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        CardSelectorPrefs prefs = new CardSelectorPrefs(new LocString("cards", "DEMOMOD-WINDS_MUSE.selectionScreenPrompt"), 0, DynamicVars["Amount"].IntValue);
        IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromHand(choiceContext, Owner, prefs, c=> true, this);
        await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, selectedCards.ToArray());
        if (selectedCards.Count() < DynamicVars["Amount"].IntValue) {
            int amount = DynamicVars["Amount"].IntValue - selectedCards.Count();
            CardPile drawPile = PileType.Draw.GetPile(Owner);
            List<CardModel> cards = [];
            for (int i = 0; i < amount; i++) {
                await CardPileCmd.ShuffleIfNecessary(choiceContext, Owner);
                CardModel cardModel = drawPile.Cards.FirstOrDefault();
                if (cardModel != null) {
                    cardModel.RemoveFromCurrentPile();
                    cards.Add(cardModel);
                } else {
                    break;
                }
            }
            await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, cards.ToArray());
        }
    }

    protected override void OnUpgrade() => DynamicVars["Amount"].UpgradeValueBy(1);
}
