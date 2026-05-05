using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Ornaments : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new PowerVar<VulnerablePower>(1),
		new DamageVar("Damage1", 1, ValueProp.Move),
		new DamageVar("Damage2", 4, ValueProp.Move)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<VulnerablePower>()
	];

	public Ornaments() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await using AttackContext context = await AttackCommand.CreateContextAsync(Owner.Creature.CombatState, this);
		await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", 0.5f);
		context.AddHit(await CreatureCmd.Damage(choiceContext, cardPlay.Target, (DamageVar) DynamicVars["Damage1"], this));
		await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, DynamicVars["VulnerablePower"].BaseValue, Owner.Creature, this);
		context.AddHit(await CreatureCmd.Damage(choiceContext, cardPlay.Target, (DamageVar) DynamicVars["Damage2"], this));
	}

	protected override void OnUpgrade() => DynamicVars["Damage2"].UpgradeValueBy(2);
}
