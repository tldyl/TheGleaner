using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Vector2 = Godot.Vector2;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class ClusterConfluence : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<StrikeGleaner>(), HoverTipFactory.FromCard<ClusterStrike>(), HoverTipFactory.FromKeyword(CustomEnums.Score)];
	protected override bool HasEnergyCostX => true;

	public ClusterConfluence() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self) {
		
	}

	public override IEnumerable<CardKeyword> CanonicalKeywords => [
		CardKeyword.Exhaust
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		for (int _ = 0; _ < ResolveEnergyXValue() + CurrentUpgradeLevel; _++) {
			CardModel strike = ModelDb.Card<StrikeGleaner>().ToMutable();
			Owner.Creature.CombatState.AddCard(strike, Owner);
			await CardPileCmd.AddGeneratedCardToCombat(strike, PileType.Hand, true);
		}
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
		await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, clusterStrike);
	}
}
