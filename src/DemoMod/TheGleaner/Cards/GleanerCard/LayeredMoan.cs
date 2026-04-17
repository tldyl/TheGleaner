using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;
[Pool(typeof(CardPool))]
public class LayeredMoan : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
		HoverTipFactory.FromKeyword(CustomEnums.Dissonance)
	];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(10, ValueProp.Move),
		new RepeatVar(1)
	];

	public LayeredMoan() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies) {
	}

	public override async Task AfterCardExhausted(
		PlayerChoiceContext choiceContext,
		CardModel card,
		bool causedByEthereal) {
		if (card is IDissonanceCard) {
			DynamicVars.Repeat.UpgradeValueBy(1);
		}
	}

	public override async Task AfterCardGeneratedForCombat(CardModel card, bool addedByPlayer) {
		if (RandomDissonanceCard.transformedDissonanceCards().Any(c => c.Id.Equals(card.Id))) {
			DynamicVars.Repeat.UpgradeValueBy(1);
		}
	}

	public override async Task AfterCardEnteredCombat(CardModel card) {
		if (card != this) {
			return;
		}

		int count = 1;
		foreach (CombatHistoryEntry entry in CombatManager.Instance.History.Entries.Where(e => e is CardExhaustedEntry or CardGeneratedEntry)) {
			switch (entry) {
				case CardExhaustedEntry {Card: IDissonanceCard}:
				case CardGeneratedEntry cardGeneratedEntry
					when RandomDissonanceCard.transformedDissonanceCards().Any(c => c.Id.Equals(cardGeneratedEntry.Card.Id)):
					count++;
					break;
			}
		}

		DynamicVars.Repeat.BaseValue = count;
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await CreatureCmd.TriggerAnim(Owner.Creature, "AoEAttack", 0.5f);
		await AfterCardEnteredCombat(this);

		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.WithHitCount(DynamicVars.Repeat.IntValue)
			.WithNoAttackerAnim()
			.TargetingAllOpponents(Owner.Creature.CombatState)
			.Execute(choiceContext);

		List<CardModel> cards = RandomDissonanceCard.getRandomDissonanceCards(
			1,
			Owner.RunState.Rng.CombatCardGeneration
		);

		foreach (CardModel card in cards) {
			PileType targetPile =
				Owner.RunState.Rng.CombatCardGeneration.NextInt(2) == 0
					? PileType.Draw
					: PileType.Discard;

			IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(
				[CombatState.CreateCard(card, Owner)],
				targetPile,
				true,
				CardPilePosition.Random
			);

			CardCmd.PreviewCardPileAdd(results);
		}
	}

	protected override void OnUpgrade() {
		DynamicVars.Damage.UpgradeValueBy(3);
	}
}
