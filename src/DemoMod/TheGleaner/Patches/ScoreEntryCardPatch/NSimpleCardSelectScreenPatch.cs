using BaseLib.Patches.Content;
using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Commands;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Runs;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace DemoMod.TheGleaner.Patches.ScoreEntryCardPatch;

public class NSimpleCardSelectScreenPatch {
    [HarmonyPatch(typeof(NSimpleCardSelectScreen), "OnCardClicked")]
    public static class PatchOnCardClicked {
        public static bool Prefix(NSimpleCardSelectScreen __instance, CardModel card) {
            if (!ScorePileCmd.openingScorePile) {
                return true;
            }
            HashSet<CardModel> _selectedCards = (HashSet<CardModel>) AccessTools.Field(typeof(NSimpleCardSelectScreen), "_selectedCards").GetValue(__instance);
            if (_selectedCards.Contains(card)) {
                return true;
            }
            RunState runState = (RunState) AccessTools.PropertyGetter(typeof(RunManager), "State").Invoke(RunManager.Instance, []);
            Player player = LocalContext.GetMe(runState.Players);
            int cost = calculateCost(player, _selectedCards.Count + 1);
            if (cost > player.PlayerCombatState.Energy) {
                NCardGrid grid = AccessTools.Field(typeof(NCardGridSelectionScreen), "_grid").GetValue(__instance) as NCardGrid;
                NCard nCard = grid.GetCardNode(card);
                WiggleAnimationWrapper animationWrapper = new WiggleAnimationWrapper {
                    animatedCard = nCard,
                    originalVisualPosition = nCard.Position.X
                };
                nCard.PlayPileTween?.Kill();
                Tween? tween = nCard.CreateTween().SetParallel();
                tween.TweenMethod(Callable.From(new Action<float>(animationWrapper.WiggleAnimation)), 0.0f, 2f, 0.3).SetEase(Tween.EaseType.Out)
                    .SetTrans(Tween.TransitionType.Quad);
                nCard.PlayPileTween = tween;
                return false;
            }
            return true;
        }

        private class WiggleAnimationWrapper {
            public NCard animatedCard;
            public float? originalVisualPosition;

            public void WiggleAnimation(float progress) {
                animatedCard.Position = animatedCard.Position with {
                    X = originalVisualPosition.Value + (float)Math.Sin(progress * 3.1415927410125732 * 2.0) * 10f
                };
            }
        }

        private static int calculateCost(Player player, int cardNum) {
            ScorePile scorePile = (ScorePile)CustomPiles.GetCustomPile(player.PlayerCombatState, CustomEnums.ScorePile);
            return cardNum - scorePile.freeTakeCount;
        }
    }
}
