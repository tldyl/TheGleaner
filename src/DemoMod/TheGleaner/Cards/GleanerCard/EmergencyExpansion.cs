using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class EmergencyExpansion : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new BlockVar(5, ValueProp.Move),
		new IntVar("Times", 3)
	];

	public EmergencyExpansion() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		int times = DynamicVars["Times"].IntValue;
		for (int i = 0; i < times; i++) {
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		}

		// This card grants one fewer block instance each time it's played in this combat, minimum 1.
		DynamicVars["Times"].BaseValue = Math.Max(1, times - 1);
	}

	protected override void OnUpgrade() => DynamicVars["Times"].UpgradeValueBy(1);
}
