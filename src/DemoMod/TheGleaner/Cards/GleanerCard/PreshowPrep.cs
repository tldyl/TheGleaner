using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands.Builders;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class PreshowPrep : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(10, ValueProp.Move),
		new IntVar("GleanAmount", 2),
		new EnergyVar("EnergyAmount", 1)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CustomEnums.Glean),
		HoverTipFactory.ForEnergy(this)
	];

	public PreshowPrep() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
				AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);

		// ✅ 改这里
		await ScorePileCmd.Glean(Owner, choiceContext, DynamicVars["GleanAmount"].BaseValue, this);

		await PowerCmd.Apply<EnergyNextTurnPower>(
			Owner.Creature,
			DynamicVars["EnergyAmount"].BaseValue,
			Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade() {
		DynamicVars.Damage.UpgradeValueBy(4);
	}
}
