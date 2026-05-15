using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using DemoMod.TheGleaner.Actions;
using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Cards.GleanerCard;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Nodes.Vfx;
using DemoMod.TheGleaner.Utils;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace TheGleaner.DemoMod.TheGleaner.Patches;

public class HookListenerModel() : CustomSingletonModel(true, true) {
    private static HookListenerModel? _instance;
    public static HookListenerModel Instance {
        get {
            return _instance ??= ModelDb.GetById<HookListenerModel>(new ModelId(ModelDb.GetCategory(typeof(HookListenerModel)), ModelDb.GetEntry(typeof(HookListenerModel))));
        }
    }

    public override bool ShouldReceiveCombatHooks => true;

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        CombatState combatState) {
        if (side == CombatSide.Player) {
            ScorePile scorePile = ScorePileCmd.GetOrCreateScorePile(LocalContext.GetMe(combatState.Players).PlayerCombatState);
            scorePile.freeTakeCount = 1;
            scorePile.cardsAddedToScoreThisTurn = false;
        }
    }
    
    public override async Task BeforeCardPlayed(CardPlay cardPlay) {
        if (cardPlay.Card.Owner.Character is global::DemoMod.TheGleaner.Characters.TheGleaner && cardPlay.Card.Type == CardType.Power) {
            await CreatureCmd.TriggerAnim(cardPlay.Card.Owner.Creature, "Cast", cardPlay.Card.Owner.Character.CastAnimDelay);
        }
    }
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
        CardModel card = cardPlay.Card;
        if (card.Keywords.Contains(CustomEnums.Resonance)) {
            Player player = cardPlay.Card.Owner;
            List<CardModel> cardsToReduceCost = [];
            cardsToReduceCost.AddRange(player.PlayerCombatState.Hand.Cards.Where(cardModel => cardModel != card && cardModel.Type != card.Type && cardModel.EnergyCost.GetResolved() > 1 && !cardModel.EnergyCost.CostsX));
            cardsToReduceCost.AddRange(player.PlayerCombatState.PlayPile.Cards.Where(cardModel => cardModel != card && cardModel.Type != card.Type && cardModel.EnergyCost.GetResolved() > 1 && !cardModel.EnergyCost.CostsX));
            foreach (CardModel model in cardsToReduceCost) {
                model.EnergyCost.SetUntilPlayed(Math.Max(model.EnergyCost.GetResolved() - 1, 1));
                if (!model.Keywords.Contains(CustomEnums.Resonance)) {
                    model.AddKeyword(CustomEnums.Resonance);
                }
                if (model is IConcertoCard concertoCard) {
                    NCard nCard = NCard.FindOnTable(model);
                    if (nCard != null) {
                        Tween tween = NCombatRoom.Instance.CreateTween().SetParallel();
                        tween.Chain().TweenCallback(Callable.From(() => {})).SetDelay(Random.Shared.NextDouble() * 0.15);
                        tween.Chain().TweenProperty(nCard, "position:y", -128.0f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
                        tween.Chain().TweenProperty(nCard, "position:y", 0.0f, 0.25).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Expo);
                        tween.Chain().TweenCallback(Callable.From(() => tween.Kill()));
                    }
                    await concertoCard.OnConcerto(player.Creature.CombatState, context, cardPlay);
                }
            }
        }
    }

    public override async Task AfterCombatVictory(CombatRoom room) {
        RandomDissonanceCard.initPool();
        foreach (Player player in RunManager.Instance.DebugOnlyGetState().Players) {
            foreach (CardModel card in ScorePileCmd.GetOrCreateScorePile(player.PlayerCombatState).Cards) {
                player.Creature.CombatState.RemoveCard(card);
            }
            ScorePileCmd.GetOrCreateScorePile(player.PlayerCombatState).Clear();
            CustomPiles.Piles.Set(player.PlayerCombatState, null);
            ScorePileCmd.hasScoreEntryCard.Set(player, false);
        }
        CustomPiles.CustomPileProviders.Remove(CustomEnums.ScorePile);
        foreach (Creature creature in room.CombatState.PlayerCreatures) {
            if (creature.Player.Character is global::DemoMod.TheGleaner.Characters.TheGleaner) {
                NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(creature);
                if (creatureNode != null) {
                    creatureNode.Visuals.GetNode<SubViewportContainer>("SubViewportContainer").Visible = false;
                }
            }
        }
        ScorePileCmd.openingScorePileAndTakeCardsToHand = false;
        ScorePileCmd.gleanCard = false;
        NGrayGradientVfxPostProcessor.Instance.ToggleBlackAndWhite(false);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side) {
        Player player = LocalContext.GetMe(RunManager.Instance.DebugOnlyGetState().Players); //只检查自己，发现需要往手牌中增加乐谱牌时再用action通知队友
        ScorePile scorePile = ScorePileCmd.GetOrCreateScorePile(player.PlayerCombatState);
        if (scorePile.Cards.Count > 0 && !NRun.Instance.CombatRoom.Ui.Hand.ActiveHolders.Any(holder => holder.CardModel is ScoreEntryCard)) {
            CardModel scoreEntryCard = ModelDb.Card<ScoreEntryCard>().ToMutable();
            scoreEntryCard.Owner = player;
            NCard nCard = NCard.Create(scoreEntryCard);
            NCombatRoom.Instance.Ui.AddChildSafely(nCard);
            nCard.Position = PileType.Hand.GetTargetPosition(nCard);
            NHandCardHolder holder = NRun.Instance.CombatRoom.Ui.Hand.Add(nCard, 0);
            holder.Hitbox.Size = new Vector2(770, 620);
            holder.Hitbox.Position = new Vector2(-350, -311);
            //如果发现自己需要新增一张乐谱，写个action通知其他队友往联机卡牌数据库中增加一张乐谱
            //如果不需要新增乐谱，则不发送这个action
            RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new EndTurnAddScoreCardAction(player, scoreEntryCard));
        }
    }
}

