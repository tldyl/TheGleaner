using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class RendezvousWithDoom : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.Static(StaticHoverTip.Energy)];
	protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];
	protected override bool HasEnergyCostX => true;


	public override IEnumerable<CardKeyword> CanonicalKeywords => [
		CardKeyword.Ethereal
	];
	
	public RendezvousWithDoom() : base(0, CardType.Power, CardRarity.Ancient, TargetType.Self) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await PowerCmd.Apply<RendezvousWithDoomPower>(Owner.Creature, ResolveEnergyXValue() + 1, Owner.Creature, this);
	}
	
	protected override void OnUpgrade() {
		RemoveKeyword(CardKeyword.Ethereal);
	}
}
