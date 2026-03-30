using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class CriticalTuning : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar("Draw", 2)
    ];

    public CriticalTuning() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        IEnumerable<CardModel> drawnCards = await CardPileCmd.Draw(choiceContext, DynamicVars["Draw"].BaseValue, Owner);
        foreach (CardModel card in drawnCards) {
            card.EnergyCost.AddThisTurnOrUntilPlayed(1);
        }
    }
    
    protected override void OnUpgrade() => DynamicVars["Draw"].UpgradeValueBy(1);
}
