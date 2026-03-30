using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Commands;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace DemoMod.TheGleaner.Utils;

public class PlayCardMock {
    public static async Task MockPlayCard(CardModel cardModel, Creature target, PlayerChoiceContext choiceContext, ResourceInfo resources) {
        CombatState combatState = cardModel.CombatState ?? cardModel.Owner.Creature.CombatState;
        choiceContext.PushModel(cardModel);
        await CombatManager.Instance.WaitForUnpause();
        AccessTools.PropertySetter(typeof(CardModel), "CurrentTarget").Invoke(cardModel, [target]);
        CardPile pile = cardModel.Pile;
        CardPileAddResult _ = await CardPileCmd.Add(cardModel, PileType.Play, skipVisuals: false);
        await Cmd.CustomScaledWait(0.25f, 0.35f);
        IEnumerable<AbstractModel> modifiers = [];
        PileType originalPileType = (PileType) AccessTools.Method(typeof(CardModel), "GetResultPileType", []).Invoke(cardModel, []);
        (PileType pileType, CardPilePosition position) =
            Hook.ModifyCardPlayResultPileTypeAndPosition(combatState, cardModel, true, resources, originalPileType, CardPilePosition.Bottom, out modifiers);
        foreach (AbstractModel abstractModel in modifiers)
            await abstractModel.AfterModifyingCardPlayResultPileOrPosition(cardModel, pileType, position);
        int playCount = cardModel.GetEnchantedReplayCount() + 1;
        List<AbstractModel> modifyingModels;
        playCount = Hook.ModifyCardPlayCount(combatState, cardModel, playCount, target, out modifyingModels);
        await Hook.AfterModifyingCardPlayCount(combatState, cardModel, modifyingModels);
        ulong playStartTime = Time.GetTicksMsec();
        for (int i = 0; i < playCount; i++) {
            if (cardModel.Type == CardType.Power) {
                await (Task) AccessTools.Method(typeof(CardModel), "PlayPowerCardFlyVfx", []).Invoke(cardModel, []);
            } else if (i > 0) {
                NCard onTable = NCard.FindOnTable(cardModel);
                if (onTable != null) {
                    await onTable.AnimMultiCardPlay();
                }
            }
            CardPlay cardPlay = new CardPlay {
                Card = cardModel,
                Target = target,
                ResultPile = pileType,
                Resources = resources,
                IsAutoPlay = true,
                PlayIndex = i,
                PlayCount = playCount
            };
            await Hook.BeforeCardPlayed(combatState, cardPlay);
            CombatManager.Instance.History.CardPlayStarted(combatState, cardPlay);
            await (Task) AccessTools.Method(typeof(CardModel), "OnPlay", [typeof(PlayerChoiceContext), typeof(CardPlay)]).Invoke(cardModel, [choiceContext, cardPlay]);
            cardModel.InvokeExecutionFinished();
            if (cardModel.Enchantment != null) {
                await cardModel.Enchantment.OnPlay(choiceContext, cardPlay);
                cardModel.Enchantment.InvokeExecutionFinished();
            }
            if (cardModel.Affliction != null) {
                AfflictionModel affliction = cardModel.Affliction;
                await affliction.OnPlay(choiceContext, target);
                affliction.InvokeExecutionFinished();
            }
            CombatManager.Instance.History.CardPlayFinished(combatState, cardPlay);
            if (CombatManager.Instance.IsInProgress) {
                await Hook.AfterCardPlayed(combatState, choiceContext, cardPlay);
            }
        }
        float num = (Time.GetTicksMsec() - playStartTime) / 1000f;
        await Cmd.CustomScaledWait(0.15f - num, 0.3f - num);

        if (pile is ScorePile) {
            await ScorePileCmd.AddCards(cardModel.Owner.PlayerCombatState, cardModel.Owner, cardModel);
        } else {
            await CardPileCmd.Add(cardModel, pileType, position, skipVisuals: false);
        }
        
        await CombatManager.Instance.CheckForEmptyHand(choiceContext, cardModel.Owner);
        Action played = AccessTools.Field(typeof(CardModel), "Played").GetValue(cardModel) as Action;
        played?.Invoke();
        choiceContext.PopModel(cardModel);
    }
}
