using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class WhenTheGrainFlowsGold : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	public override IEnumerable<CardKeyword> CanonicalKeywords => [
		CardKeyword.Exhaust
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CustomEnums.Score)
	];

	public WhenTheGrainFlowsGold() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		List<CardModel> selectedCards = (
			await ScorePileCmd.ShowScorePileScreen(
				Owner.PlayerCombatState,
				choiceContext,
				Owner,
				true
			)
		).ToList();

		if (selectedCards.Count > 0) {
			await CardCmd.Discard(choiceContext, selectedCards);

			foreach (CardModel card in selectedCards) {
				if (Owner.Creature.HasPower<StaffBurnoutPower>()) {
					await Owner.Creature.GetPower<StaffBurnoutPower>().AfterCardChangedPiles(card, CustomEnums.ScorePile, null);
				}
			}
		}

		int amountToFill = ScorePileCmd.GetCapacity(Owner) - CustomEnums.ScorePile.GetPile(Owner).Cards.Count;

		for (int i = 0; i < amountToFill; i++) {
			await CardPileCmd.ShuffleIfNecessary(choiceContext, Owner);

			if (PileType.Draw.GetPile(Owner).Cards.Count <= 0) {
				break;
			}

			await ScorePileCmd.AddCards(
				Owner.PlayerCombatState,
				Owner,
				PileType.Draw.GetPile(Owner).Cards[0]
			);
		}
	}

	protected override void OnUpgrade() {
		EnergyCost.UpgradeBy(-1);
	}
}