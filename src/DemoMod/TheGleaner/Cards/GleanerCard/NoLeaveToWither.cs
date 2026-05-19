using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using DemoMod.TheGleaner.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class NoLeaveToWither : CustomCardModel
{
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new IntVar("Amount", 1)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CustomEnums.Dissonance),
		HoverTipFactory.FromPower<DoomPower>(),
		HoverTipFactory.Static(StaticHoverTip.Block),
		HoverTipFactory.FromCard<DirgeOfFarewell>(),
		HoverTipFactory.FromCard<ShriekOfDread>(),
		HoverTipFactory.FromCard<HowlOfWrath>()
	];

	public override IEnumerable<CardKeyword> CanonicalKeywords => [
		CardKeyword.Exhaust
	];

	public NoLeaveToWither() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<NoLeaveToWitherPower>(
			Owner.Creature,
			1,
			Owner.Creature,
			this
		);

		List<CardModel> cards = RandomDissonanceCard.getRandomDissonanceCards(
			DynamicVars["Amount"].IntValue,
			Owner.RunState.Rng.CombatCardGeneration
		);

		SoundManager.Instance.PlaySound(SoundKeys.HEART_BEAT);
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
		EnergyCost.UpgradeBy(-1);
	}
}
