using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Inspiration : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new IntVar("Amount", 2)
	];

	public override IEnumerable<CardKeyword> CanonicalKeywords => [
		CardKeyword.Exhaust
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<StrengthPower>(),
		HoverTipFactory.FromPower<DexterityPower>(),
		HoverTipFactory.FromKeyword(CustomEnums.Score)
	];

	public Inspiration() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {

		// ===== 力量（参考 HarmonicPillar）=====

		await PowerCmd.Apply<FlexPotionPower>(
			Owner.Creature,
			DynamicVars["Amount"].BaseValue,
			Owner.Creature,
			this
		);

		// ===== 敏捷（参考 CascadingStrings）=====
		await PowerCmd.Apply<SpeedPotionPower>(
			Owner.Creature,
			DynamicVars["Amount"].BaseValue,
			Owner.Creature,
			this
		);
	}

	public override async Task BeforeCombatStart() {
		if (!IsInCombat || CombatState == null || Owner.Deck.Cards.Contains(this)) {
			return;
		}

		await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
	}   

	protected override void OnUpgrade() {
		DynamicVars["Amount"].UpgradeValueBy(1);
	}
}
