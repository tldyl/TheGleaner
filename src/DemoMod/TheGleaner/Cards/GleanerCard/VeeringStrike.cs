using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;
[Pool(typeof(CardPool))]
public class VeeringStrike : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(4, ValueProp.Move),
		new RepeatVar(3)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CustomEnums.Score)
	];

	public VeeringStrike() : base(2, CardType.Attack, CardRarity.Common, TargetType.RandomEnemy) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.WithHitCount(DynamicVars.Repeat.IntValue)
			.FromCard(this)
			.TargetingRandomOpponents(CombatState, true)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
	}

	public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side) {
		CardPile? pile = Pile;

		if (pile != null && pile.Type != CustomEnums.ScorePile || side != Owner.Creature.Side || pile == null) {
			return;
		}

		await CardCmd.AutoPlay(choiceContext, this, null);
		if (pile.Cards.Count == 0 && NRun.Instance.CombatRoom.Ui.Hand.ActiveHolders.Any(holder => holder.CardModel is ScoreEntryCard)) {
			NRun.Instance.CombatRoom.Ui.Hand.Remove(
				NRun.Instance.CombatRoom.Ui.Hand.ActiveHolders.FirstOrDefault(holder => holder.CardModel is ScoreEntryCard).CardModel
			);
		}
	}

	protected override void OnUpgrade() {
		DynamicVars.Repeat.UpgradeValueBy(1);
	}
}
