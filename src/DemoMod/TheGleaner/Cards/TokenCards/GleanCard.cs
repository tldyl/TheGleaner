using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace DemoMod.TheGleaner.Cards.TokenCards;

[Pool(typeof(TokenCardPool))]
public class GleanCard : CustomCardModel, IChoosable {
    public override bool CanBeGeneratedInCombat => false;
    public override int MaxUpgradeLevel => 0;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Glean)];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar("Amount", 1)
    ];

    public GleanCard() : base(-1, CardType.Skill, CardRarity.Token, TargetType.None) {
        
    }

    public async Task OnChosen(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await ScorePileCmd.Glean(cardPlay.Card.Owner, choiceContext, DynamicVars["Amount"].BaseValue, cardPlay.Card);
    }
}
