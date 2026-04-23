using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class StringAndPillar : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Amount", 1), new EnergyVar(1)];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.FromPower<DexterityPower>()];
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate, CardKeyword.Retain, CustomEnums.Resonance];

	public StringAndPillar() : base(5, CardType.Power, CardRarity.Basic, TargetType.Self) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await PowerCmd.Apply<StrengthPower>(Owner.Creature, DynamicVars["Amount"].BaseValue, Owner.Creature, this);
		await PowerCmd.Apply<DexterityPower>(Owner.Creature, DynamicVars["Amount"].BaseValue, Owner.Creature, this);

		if (IsUpgraded) {
			CardModel cpy = CreateClone();
			CardCmd.Downgrade(cpy);
			AccessTools.Field(typeof(CardModel), "_energyCost").SetValue(cpy, new CardEnergyCost(cpy, 5, false));
			await CardPileCmd.AddGeneratedCardToCombat(cpy, PileType.Hand, true);
		}
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
		if (cardPlay.Card.Type != Type && EnergyCost.GetResolved() > 1 && cardPlay.Card is not ScoreEntryCard && cardPlay.Card.Owner == Owner) {
			EnergyCost.AddThisCombat(-1);
		}
	}
}
