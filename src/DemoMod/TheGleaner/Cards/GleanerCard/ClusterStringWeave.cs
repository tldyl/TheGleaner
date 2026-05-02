using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Utils;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class ClusterStringWeave : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	private CardModel previewCard = ModelDb.Card<ClusterStrike>().ToMutable();

	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard(previewCard), HoverTipFactory.FromKeyword(CustomEnums.Score)];

	public ClusterStringWeave() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
	}

	public override async Task BeforeCombatStart() {
		if (!IsInCombat || CombatState == null || Owner.Deck.Cards.Contains(this)) {
			return;
		}

		CardCmd.Preview(this);
		await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		List<CardModel> mergedCards = PileType.Hand.GetPile(Owner).Cards
			.Where(c => c.Tags.Contains(CardTag.Strike) || c.Tags.Contains(CustomEnums.Arrow))
			.ToList();

		CardPile pile = ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState);
		if (pile != null) {
			List<CardModel> toRemove = pile.Cards
				.Where(c => c.Tags.Contains(CardTag.Strike) || c.Tags.Contains(CustomEnums.Arrow))
				.ToList();

			foreach (CardModel card in toRemove) {
				mergedCards.Add(card);
			}
		}

		if (mergedCards.Count > 1) {
			List<NCard> cardVisuals = [];
			foreach (CardModel card in mergedCards) {
				if (Owner.PlayerCombatState.Hand.Cards.Contains(card)) {
					NCard tmp = NCard.FindOnTable(card) ?? NCard.Create(card);
					NCard nCard = NCard.Create(card);
					nCard.GlobalPosition = new Vector2(tmp.GlobalPosition.X, tmp.GlobalPosition.Y);
					if (NRun.Instance.CombatRoom.Ui.Hand.GetCardHolder(card) != null) {
						NRun.Instance.CombatRoom.Ui.Hand.Remove(card);
					}
					cardVisuals.Add(nCard);
				} else {
					NCard nCard = NCard.Create(card);
					nCard.GlobalPosition = card.Pile.Type.GetTargetPosition(nCard);
					cardVisuals.Add(nCard);
				}
			}
			foreach (NCard nCard in cardVisuals) {
				Node node = nCard.Model.Pile.Type != PileType.Deck ? NCombatRoom.Instance.CombatVfxContainer : NRun.Instance.GlobalUi.TopBar.TrailContainer;
				node.AddChild(nCard);
				nCard.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
				NCardFlyVfx child = NCardFlyVfx.Create(nCard, PileType.Play.GetTargetPosition(nCard), false, nCard.Model.Owner.Character.TrailPath);
				node.AddChildSafely((Node) child);
			}
			await CardPileCmd.RemoveFromCombat(mergedCards, true);

			GleanerVfxCmd.CheckScoreIsEmpty(Owner.PlayerCombatState);

			ClusterStrike clusterStrike = (ClusterStrike)ModelDb.Card<ClusterStrike>().ToMutable();

			clusterStrike.setCards(mergedCards);
			Log.Info($"clusterStrike netId: {NetCombatCardDb.Instance.IdCardForTesting(clusterStrike)}");
			Owner.Creature.CombatState.AddCard(clusterStrike, Owner);
			await CardPileCmd.AddGeneratedCardToCombat(clusterStrike, PileType.Play, true);
			if (LocalContext.IsMe(Owner)) {
				TaskHelper.RunSafely(playVfx(clusterStrike));
			} else {
				clusterStrike.RemoveFromCurrentPile();
				await CardPileCmd.Add(clusterStrike, PileType.Hand.GetPile(Owner));
			}
		}
	}

	private async Task playVfx(CardModel clusterStrike) {
		Tween tween = NCombatRoom.Instance.CreateTween().SetParallel();
		tween.Chain().TweenCallback(Callable.From(() => {
			NCardTransformVfx vfx = NCardTransformVfx.Create(this, clusterStrike, null);
			vfx.GlobalPosition = NGame.Instance.GetViewportRect().Size * 0.5f - NCard.Create(this).Size * 0.5f + Vector2.Up * 100f;
			vfx.Scale = new Vector2(0.9f, 0.9f);
			NCombatRoom.Instance.Ui.AddChildSafely(vfx);
		}));
		tween.Chain().TweenCallback(Callable.From(() => {
		})).SetDelay(2.8f);
		await Cmd.Wait(2.7f);
		clusterStrike.RemoveFromCurrentPile();
		PileType destPile = PileType.Discard;
		if (!Hook.ShouldFlush(Owner.Creature.CombatState, Owner)) {
			destPile = PileType.Hand;
		}
		await CardPileCmd.Add(clusterStrike, (CombatManager.Instance.IsEnemyTurnStarted ? destPile : PileType.Hand).GetPile(Owner));
	}
	protected override void OnUpgrade() {
		EnergyCost.UpgradeBy(-1);
	}
}
