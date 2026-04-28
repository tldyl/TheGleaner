using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(TokenCardPool))]
public class TestAttack : CustomCardModel {
	public override bool HasBuiltInOverlay => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(6, ValueProp.Move)
	];

	public TestAttack() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		AttackCommand attackCommand = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);
	}

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1);
}