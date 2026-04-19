using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class TwiningTone : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(7, ValueProp.Move),
		new ExtraDamageVar(2)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Score)];

	public TwiningTone() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);
	}

	public override decimal ModifyDamageAdditive(
		Creature? target,
		decimal amount,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource) {
		if (cardSource == this && !props.HasFlag(ValueProp.Unpowered)) {
			if (Owner?.PlayerCombatState == null) {
				return 0m;
			}

			CardPile? scorePile = ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState);
			int scoreCount = scorePile?.Cards.Count ?? 0;

			return scoreCount * DynamicVars.ExtraDamage.BaseValue;
		}

		return 0m;
	}

	protected override void OnUpgrade() {
		DynamicVars.Damage.UpgradeValueBy(3);	
	}
}
