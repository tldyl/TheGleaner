using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Collapse : CustomCardModel
{
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CustomEnums.Dissonance),
		HoverTipFactory.FromCard<DirgeOfFarewell>(),
		HoverTipFactory.FromCard<ShriekOfDread>(),
		HoverTipFactory.FromCard<HowlOfWrath>()
	];

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(9, ValueProp.Move),
		new IntVar("DissonanceAmount", 1)
	];

	public Collapse() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.TargetingRandomOpponents(Owner.Creature.CombatState)
			.WithHitCount(Owner.Creature.CombatState.HittableEnemies.Count + 1)
			.Execute(choiceContext);

		List<CardModel> cards = RandomDissonanceCard.getRandomDissonanceCards(
			DynamicVars["DissonanceAmount"].IntValue,
			Owner.RunState.Rng.CombatCardGeneration
		);

		foreach (CardModel card in cards)
		{
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

			CardCmd.PreviewCardPileAdd(
				results,
				1.2f,
				MegaCrit.Sts2.Core.Nodes.CommonUi.CardPreviewStyle.HorizontalLayout
			);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(2);
	}
}
