using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using DemoMod.TheGleaner.Utils;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class CrashingDown : CustomCardModel
{
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(40, ValueProp.Move),
		new IntVar("powerVal", 1)
	];

	public CrashingDown() : base(4, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.WithNoAttackerAnim()
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);
		await PowerCmd.Apply<WeakPower>(
			Owner.Creature.CombatState.HittableEnemies,
			DynamicVars["powerVal"].BaseValue,
			Owner.Creature,
			this
		);
		await PowerCmd.Apply<VulnerablePower>(
			Owner.Creature.CombatState.HittableEnemies,
			DynamicVars["powerVal"].BaseValue,
			Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade(){
		DynamicVars.Damage.UpgradeValueBy(10);
		DynamicVars["powerVal"].UpgradeValueBy(1);
	} 
}
