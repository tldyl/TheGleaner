using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;
using BaseLib.Patches.Content;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Phantasm : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	public override bool GainsBlock => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		EnergyHoverTip
	];

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new BlockVar(7, ValueProp.Move),
		new IntVar("Times", 2),
		new EnergyVar(1)
	];

	public Phantasm() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true, true) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		int times = DynamicVars["Times"].IntValue;
		for (int i = 0; i < times; i++) {
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		}
	}

	public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side) {
		if (side != CombatSide.Player) {
			return;
		}
		
		CardPile scorePile = CustomPiles.GetCustomPile(Owner.PlayerCombatState, CustomEnums.ScorePile);

		if (scorePile != null && scorePile.Cards.Contains(this)) {
			EnergyCost.AddUntilPlayed(-1);
			return;
		}

		if (Owner.PlayerCombatState.Hand.Cards.Contains(this)) {
			await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
		}
	}

	protected override void OnUpgrade() {
		  DynamicVars.Block.UpgradeValueBy(2);
	}
}
