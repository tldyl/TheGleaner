using BaseLib.Abstracts;
using DemoMod.TheGleaner.Cards.GleanerCard;
using DemoMod.TheGleaner.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Powers;

public class SentientMusicalNotePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => DynamicVars["CardsLeft"].IntValue;
    public override bool IsInstanced => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("CardsLeft", 3M)
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (RandomDissonanceCard.transformedDissonanceCards().Any(c => c.Id.Equals(cardPlay.Card.Id)))
        {
            DynamicVars["CardsLeft"].BaseValue--;
        }

        if (DynamicVars["CardsLeft"].IntValue <= 0)
        {
            for (int _ = 0; _ < Amount; _++)
            {
                await CardPileCmd.AddGeneratedCardToCombat(
                    CombatState.CreateCard(ModelDb.Card<RoundAndRound>(), Owner.Player),
                    PileType.Hand,
                    true
                );
            }

            DynamicVars["CardsLeft"].BaseValue = 3M;
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)
        {
            return;
        }

        List<CardModel> cards = RandomDissonanceCard.getRandomDissonanceCards(
            Amount,
            Owner.Player.RunState.Rng.CombatCardGeneration
        );

        foreach (CardModel card in cards)
        {
            PileType targetPile =
                Owner.Player.RunState.Rng.CombatCardGeneration.NextInt(2) == 0
                    ? PileType.Draw
                    : PileType.Discard;

            CardCmd.PreviewCardPileAdd(
                await CardPileCmd.AddGeneratedCardToCombat(
                    CombatState.CreateCard(card, Owner.Player),
                    targetPile,
                    true
                )
            );
        }
    }
}