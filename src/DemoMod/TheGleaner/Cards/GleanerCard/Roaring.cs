using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Roaring : CustomCardModel {
	//public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new BlockVar(6, ValueProp.Move),
		new IntVar("DissonanceAmount", 1)
	];
	public override bool GainsBlock => true;
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CustomEnums.Dissonance),
		HoverTipFactory.FromCard<DirgeOfFarewell>(),
		HoverTipFactory.FromCard<ShriekOfDread>(),
		HoverTipFactory.FromCard<HowlOfWrath>()
	];

	public Roaring() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		List<CardModel> cards = RandomDissonanceCard.getRandomDissonanceCards(
			DynamicVars["DissonanceAmount"].IntValue,
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
				Owner,
				CardPilePosition.Random
			);

			CardCmd.PreviewCardPileAdd(
				results,
				1.2f,
				MegaCrit.Sts2.Core.Nodes.CommonUi.CardPreviewStyle.HorizontalLayout
			);
		}
	}

	protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2);
}
