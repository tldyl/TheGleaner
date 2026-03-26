using DemoMod.TheGleaner.Cards.GleanerCard;
using DemoMod.TheGleaner.Enums;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Patches;
public class HookPatch {
    [HarmonyPatch(typeof(Hook), "AfterCardPlayed")]
    public static class PatchAfterCardPlayed {
        public static void Prefix(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            CardModel card = cardPlay.Card;
            if (card.Keywords.Contains(CustomEnums.Resonance)) {
                Player player = LocalContext.GetMe(combatState.Players);
                List<CardModel> cardsToReduceCost = [];
                cardsToReduceCost.AddRange(player.PlayerCombatState.Hand.Cards.Where(cardModel => cardModel != card && cardModel.Type != card.Type && cardModel.EnergyCost.GetResolved() > 1));
                cardsToReduceCost.ForEach(model => {
                    if (model is IConcertoCard concertoCard) {
                        concertoCard.OnConcerto(combatState, choiceContext, cardPlay);
                    } else {
                        model.EnergyCost.SetThisCombat(model.EnergyCost.GetResolved() - 1);
                    }
                    if (!model.Keywords.Contains(CustomEnums.Resonance)) {
                        model.AddKeyword(CustomEnums.Resonance);
                    }
                });
            }
        }
    }
}
