using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Swing : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public Swing() : base(2, CardType.Skill, CardRarity.Common, TargetType.RandomEnemy) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await AutoplayRandomCardInPile(PileType.Draw, choiceContext);
        await AutoplayRandomCardInPile(PileType.Hand, choiceContext);

        if (CurrentUpgradeLevel > 0) {
            await AutoplayRandomCardInPile(PileType.Discard, choiceContext);
        }
    }

    private async Task AutoplayRandomCardInPile(PileType pileType, PlayerChoiceContext choiceContext) {
        CardModel card1 = pileType.GetPile(Owner).Cards
            .Where(c => c.Type == CardType.Attack && !c.Keywords.Contains(CardKeyword.Unplayable)).ToList()
            .StableShuffle(Owner.RunState.Rng.Shuffle)
            .FirstOrDefault() ?? PileType.Draw.GetPile(Owner).Cards.Where(c => c.Type == CardType.Attack).ToList()
            .StableShuffle(Owner.RunState.Rng.Shuffle).FirstOrDefault();
        if (card1 != null) {
            await CardCmd.AutoPlay(choiceContext, card1, null);
        }
    }
}
