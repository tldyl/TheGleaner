using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class EmergencyExpansion : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8, ValueProp.Move)];

	public override bool GainsBlock => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CustomEnums.Score)
	];

	public override async Task BeforeCombatStart() {
		if (!IsInCombat || CombatState == null || Owner.Deck.Cards.Contains(this)) {
			return;
		}
		
		CardCmd.Preview(this);
		await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
	}
	
	public EmergencyExpansion() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		if (CurrentUpgradeLevel > 0) {
			CardModel cpy = CreateClone();
			cpy.DowngradeInternal();
			await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, cpy);
			CardCmd.Preview(cpy);
		}
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
	}
		protected override void OnUpgrade() {
		AddKeyword(CardKeyword.Exhaust);
	}
}
