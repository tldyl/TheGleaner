using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class StoreAndRelease : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.Static(StaticHoverTip.Block)
	];

public override IEnumerable<CardKeyword> CanonicalKeywords => [
		CardKeyword.Exhaust
	];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4, ValueProp.Move)];

	private int blockedDamage;
	
	public StoreAndRelease() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		AttackContext context = await AttackCommand.CreateContextAsync(Owner.Creature.CombatState, this);
		await using (context) {
			IEnumerable<DamageResult> damageResults = await CreatureCmd.Damage(choiceContext, Owner.Creature.CombatState.HittableEnemies, DynamicVars.Damage, Owner.Creature, this);
			context.AddHit(damageResults);
		}
	}

	public override async Task BeforeCombatStart() {
		if (!IsInCombat || CombatState == null) {
			return;
		}

		await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
	}
	
	public override async Task AfterDamageReceived(
		PlayerChoiceContext choiceContext,
		Creature target,
		DamageResult damageResult,
		ValueProp props,
		Creature? dealer,
		CardModel? _) {
		ScorePile scorePile = ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState);
		if (!scorePile.Cards.Contains(this) || target != Owner.Creature) {
			return;
		}
		blockedDamage += damageResult.BlockedDamage;
	}

	public override Decimal ModifyDamageAdditive(
		Creature? target,
		Decimal amount,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource) {
		if (cardSource == this && !props.HasFlag(ValueProp.Unpowered)) {
			return blockedDamage;
		}
		return 0M;
	}
	
	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}
