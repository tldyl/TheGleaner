using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;
[Pool(typeof(CardPool))]
public class Sonotoxin : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new PowerVar<PoisonPower>(4)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CustomEnums.Score)
	];

	public Sonotoxin() : base(2, CardType.Skill, CardRarity.Common, TargetType.AllEnemies) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		 await PowerCmd.Apply<PoisonPower>(Owner.Creature.CombatState.HittableEnemies, DynamicVars["PoisonPower"].BaseValue, Owner.Creature, this);
	}

	public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side) {
		CardPile? pile = Pile;

		if (pile != null && pile.Type != CustomEnums.ScorePile || side != Owner.Creature.Side || pile == null) {
			return;
		}

		await CardCmd.AutoPlay(choiceContext, this, null);
		GleanerVfxCmd.CheckScoreIsEmpty(Owner.PlayerCombatState);
	}

	protected override void OnUpgrade() {
		DynamicVars["PoisonPower"].UpgradeValueBy(2);
	}
}
