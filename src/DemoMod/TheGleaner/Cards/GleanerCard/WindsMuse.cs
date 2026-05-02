using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class WindsMuse : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Amount", 3)];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Glean)];


	public override IEnumerable<CardKeyword> CanonicalKeywords => [
		CardKeyword.Exhaust
	];
	
	public WindsMuse() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await ScorePileCmd.Glean(Owner, choiceContext, DynamicVars["Amount"].BaseValue, this);
	}

	protected override void OnUpgrade() {
		DynamicVars["Amount"].UpgradeValueBy(1);
	}
}
