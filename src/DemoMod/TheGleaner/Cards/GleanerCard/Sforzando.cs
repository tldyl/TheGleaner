using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Sforzando : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block), HoverTipFactory.FromKeyword(CustomEnums.Score)];

    public Sforzando() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        CardPile drawPile = PileType.Draw.GetPile(Owner);
        CardModel? selectedCard = null;
        if (drawPile.Cards.Count == 0) {
        } else {
            CardSelectorPrefs prefs = new CardSelectorPrefs(new LocString("cards", "DEMOMOD-SFORZANDO.selectionScreenPromptDraw"), 0, 1);
            selectedCard = (await CardSelectCmd.FromSimpleGrid(choiceContext, drawPile.Cards.Where(c => c.Type == CardType.Attack).ToList(), Owner, prefs)).FirstOrDefault();
        }
        if (selectedCard != null) {
            await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, selectedCard);
        } else {
            CardPile discardPile = PileType.Discard.GetPile(Owner);
            if (discardPile.Cards.Count > 0) {
                CardSelectorPrefs prefs = new CardSelectorPrefs(new LocString("cards", "DEMOMOD-SFORZANDO.selectionScreenPromptDiscard"), 1);
                selectedCard = (await CardSelectCmd.FromSimpleGrid(choiceContext, discardPile.Cards.Where(c => c.Type == CardType.Attack).ToList(), Owner, prefs)).FirstOrDefault();
                if (selectedCard != null) {
                    await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, selectedCard);
                }
            }
        }
        await PowerCmd.Apply<SforzandoPower>(Owner.Creature, 1, Owner.Creature, this);
    }
    
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
