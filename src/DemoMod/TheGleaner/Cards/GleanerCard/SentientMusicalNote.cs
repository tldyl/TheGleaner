using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class SentientMusicalNote : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new EnergyVar("Energy", 2)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.ForEnergy(this),
		HoverTipFactory.FromKeyword(CustomEnums.Phrase),
		HoverTipFactory.FromKeyword(CustomEnums.Dissonance),
		HoverTipFactory.FromCard<DirgeOfFarewell>(),
		HoverTipFactory.FromCard<ShriekOfDread>(),
		HoverTipFactory.FromCard<HowlOfWrath>()
	];

	public SentientMusicalNote() : base(0, CardType.Power, CardRarity.Rare, TargetType.Self) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await PlayerCmd.GainEnergy(DynamicVars["Energy"].BaseValue, Owner);
		await PowerCmd.Apply<SentientMusicalNotePower>(Owner.Creature, 1, Owner.Creature, this);
	}

	protected override void OnUpgrade() {
		DynamicVars["Energy"].UpgradeValueBy(1);
	}
}
