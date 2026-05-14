using DemoMod.TheGleaner.Cards.GleanerCard;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Random;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DemoMod.TheGleaner.Patches.ScoreEntryCardPatch;

public class CardCmdPatch {
    [HarmonyPatch(typeof(CardCmd), "DiscardAndDraw")]
    public static class PatchDiscardAndDraw {
        public static void Prefix(PlayerChoiceContext choiceContext,
            ref IEnumerable<CardModel> cardsToDiscard,
            ref int cardsToDraw) {
            List<CardModel> cardModels = cardsToDiscard.ToList();
            int count = cardModels.Count;
            cardsToDiscard = cardModels.Where(model => model is not ScoreEntryCard);
            if (cardsToDiscard.ToList().Count != count && cardsToDraw > 0) {
                cardsToDraw--;
            }
        }
    }

    // [HarmonyPatch(typeof(CardCmd), "Exhaust")]
    // public static class PatchExhaust {
    //     public static bool Prefix(ref Task __result, PlayerChoiceContext choiceContext,
    //         CardModel card,
    //         bool causedByEthereal,
    //         bool skipVisuals) {
    //         __result = Task.CompletedTask;
    //         return card is ScoreEntryCard;
    //     }
    // }

    // [HarmonyPatch(typeof(CardCmd), "Transform", typeof(IEnumerable<CardTransformation>), typeof(Rng), typeof(CardPreviewStyle))]
    // public static class PatchTransform {
    //     public static bool Prefix(ref Task<IEnumerable<CardPileAddResult>> __result, ref IEnumerable<CardTransformation> transformations,
    //         Rng? rng,
    //         CardPreviewStyle style) {
    //         List<CardTransformation> transformationList = [];
    //         transformationList.AddRange(transformations.Where(cardTransformation => cardTransformation.Original is not ScoreEntryCard));
    //         transformations = transformationList;
    //         return false;
    //     }
    // }
}
