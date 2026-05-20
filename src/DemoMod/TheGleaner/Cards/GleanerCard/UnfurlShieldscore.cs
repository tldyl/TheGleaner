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
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class UnfurlShieldscore : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new IntVar("GleanAmount", 1),
		new PowerVar<UnfurlShieldscorePower>(2)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<DexterityPower>(),
		HoverTipFactory.FromKeyword(CustomEnums.Glean),
		HoverTipFactory.Static(StaticHoverTip.Block),
		HoverTipFactory.FromKeyword(CustomEnums.Score)
	];

	public UnfurlShieldscore() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await PowerCmd.Apply<UnfurlShieldscorePower>(Owner.Creature, DynamicVars["UnfurlShieldscorePower"].BaseValue, Owner.Creature, this);
		await ScorePileCmd.Glean(Owner, choiceContext, DynamicVars["GleanAmount"].BaseValue, this);
	}
	
	protected override void OnUpgrade() => DynamicVars["GleanAmount"].UpgradeValueBy(2);
}
