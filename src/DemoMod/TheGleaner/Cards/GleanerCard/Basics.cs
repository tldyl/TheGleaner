using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Basics : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar("Amount", 1)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Glean)];

    public Basics() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        CardSelectorPrefs prefs = new CardSelectorPrefs(new LocString("cards", ModelDb.GetId<Wish>().Entry + ".selectionScreenPrompt"), 1);
        CardModel card = (await CardSelectCmd.FromSimpleGrid(choiceContext, PileType.Draw.GetPile(Owner).Cards.OrderBy(c => c.Rarity).ThenBy(c => c.Id).ToList(), Owner, prefs)).FirstOrDefault();
        if (card == null)
            return;
        await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, card);
        if (card is IDissonanceCard dissonanceCard) {
            dissonanceCard.TransformFollowupAction = c => {
                CardCmd.Upgrade(c);
            };
        } else {
            CardCmd.Upgrade(card);
        }
    }
    
    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);
}
