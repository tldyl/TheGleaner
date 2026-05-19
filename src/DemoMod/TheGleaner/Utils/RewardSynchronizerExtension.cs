using BaseLib.Abstracts;
using DemoMod.TheGleaner.Messages;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace DemoMod.TheGleaner.Utils;

public static class RewardSynchronizerExtension {
    public static async Task<IEnumerable<CardModel>> DoLocalCardTransform(this RewardSynchronizer rewardSynchronizer, int transformAmount, Func<CardModel, bool>? filter = null) {
        INetGameService _gameService = AccessTools.Field(typeof(RewardSynchronizer), "_gameService").GetValue(rewardSynchronizer) as INetGameService;
        _gameService.SendMessage(new CustomMessageWrapper {
            Message = new CardTransformMessage {
                transformAmount = transformAmount
            }
        });
        Player player = (Player) AccessTools.PropertyGetter(typeof(RewardSynchronizer), "LocalPlayer").Invoke(rewardSynchronizer, []);
        return await rewardSynchronizer.DoCardTransform(player, transformAmount, filter);
    }

    public static async Task<IEnumerable<CardModel>> DoCardTransform(this RewardSynchronizer rewardSynchronizer, Player player, int transformAmount, Func<CardModel, bool>? filter = null) {
        IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromDeckForTransformation(player, new CardSelectorPrefs(new LocString("gameplay_ui", "THE_GLEANER_COMBAT_REWARD_CARD_TRANSFORM.selectionScreenPrompt"), transformAmount) {
            Cancelable = true,
            RequireManualConfirmation = true
        });
        if (selectedCards == null)
            return [];
        foreach (CardModel card in selectedCards) {
            await CardCmd.TransformToRandom(card, player.RunState.Rng.Niche);
        }
        return selectedCards;
    }
}
