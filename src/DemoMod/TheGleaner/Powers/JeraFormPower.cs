using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using DemoMod.TheGleaner.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace DemoMod.TheGleaner.Powers;

public class JeraFormPower : CustomPowerModel {
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DissonanceAmount", 1)
    ];

    public override async Task AfterModifyingPowerAmountReceived(PowerModel power) {
        if (power is not JeraFormPower) {
            return;
        }

        if (Amount > 0) {
            DynamicVars["DissonanceAmount"].UpgradeValueBy(1);
        }
    }

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player) {
        if (player.Creature != Owner) {
            return;
        }

        CardPile pile = CustomPiles.GetCustomPile(player.PlayerCombatState, CustomEnums.ScorePile);
        if (pile == null || pile.Cards.Count == 0) {
            return;
        }

        List<CardModel> selectedCards = [];
        if (pile.Cards.Count > Amount) {
            CardSelectorPrefs prefs = new CardSelectorPrefs(
                new LocString("powers", "DEMOMOD-JERA_FORM_POWER.selectionScreenPrompt"),
                Amount
            );

            IEnumerable<CardModel> selected = await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                pile.Cards,
                player,
                prefs
            );

            selectedCards = selected.ToList();
        } else {
            selectedCards = pile.Cards.ToList();
        }

        ResourceInfo resources = new ResourceInfo
        {
            EnergySpent = 0,
            EnergyValue = 0,
            StarsSpent = 0,
            StarValue = 0
        };

        for (int i = 0; i < Amount; i++) {
            foreach (CardModel selectedCard in selectedCards) {
                IReadOnlyList<Creature> hittableEnemies = CombatState.HittableEnemies;
                if (hittableEnemies.Count == 0) {
                    return;
                }

                Creature target = player.RunState.Rng.CombatTargets.NextItem(hittableEnemies);
                await PlayCardMock.MockPlayCard(selectedCard, target, choiceContext, resources);
            }
        }
        GleanerVfxCmd.CheckScoreIsEmpty(player.PlayerCombatState);

        List<CardModel> cards = RandomDissonanceCard.getRandomDissonanceCards(
            DynamicVars["DissonanceAmount"].IntValue,
            player.RunState.Rng.CombatCardGeneration
        );

        foreach (CardModel card in cards) {
            PileType targetPile =
                player.RunState.Rng.CombatCardGeneration.NextInt(2) == 0
                    ? PileType.Draw
                    : PileType.Discard;

            CardCmd.PreviewCardPileAdd(
                await CardPileCmd.AddGeneratedCardToCombat(
                    CombatState.CreateCard(card, player),
                    targetPile,
                    true
                )
            );
        }
    }
}
