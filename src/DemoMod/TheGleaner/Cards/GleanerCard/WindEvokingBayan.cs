using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class WindEvokingBayan : CustomCardModel {
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new BlockVar(4, ValueProp.Move)
	];
	public override bool GainsBlock => true;
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	
	public WindEvokingBayan() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
	}
	
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CustomEnums.Resonance];
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
	}
	
	protected override void OnUpgrade() {
		DynamicVars.Block.UpgradeValueBy(3);
	}
}
