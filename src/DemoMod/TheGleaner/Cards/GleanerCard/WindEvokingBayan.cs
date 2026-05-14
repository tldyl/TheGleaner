using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class WindEvokingBayan : CustomCardModel {
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new BlockVar(6, ValueProp.Move),
		new IntVar("Amount", 2)
	];
	public override bool GainsBlock => true;
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	
	public WindEvokingBayan() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) {
	}
	
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CustomEnums.Resonance];
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		await PowerCmd.Apply<DemoTempStrengthPower>(Owner.Creature, DynamicVars["Amount"].BaseValue, Owner.Creature, this);
	}
	
	protected override void OnUpgrade() {
		DynamicVars.Block.UpgradeValueBy(2);
		DynamicVars["Amount"].UpgradeValueBy(1);
	}
}
