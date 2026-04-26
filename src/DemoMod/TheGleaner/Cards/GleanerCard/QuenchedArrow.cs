using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Nodes.Vfx;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class QuenchedArrow : CustomCardModel, IArrowCard {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new IntVar("Amount", 0),
		new IntVar("Grow", 50),
		new DamageVar(14, ValueProp.Move)
	];
	protected override HashSet<CardTag> CanonicalTags => [CustomEnums.Arrow];

	public QuenchedArrow() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		if (!cardPlay.IsAutoPlay) {
			GleanerVfxCmd.PlayOnCreature(cardPlay.Target, "res://TheGleaner/scenes/vfx/arrow_attack.tscn", 0.3f);
			await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", 0.5f);
			GleanerVfxCmd.PlayOnCreature(cardPlay.Target, "res://TheGleaner/scenes/vfx/arrow_hit_vfx.tscn");
		} else {
			NPrismaticRainfallStrikeVfx vfx = NPrismaticRainfallStrikeVfx.Create(cardPlay.Target);
			GleanerVfxCmd.PlayVfx(new Vector2(), vfx);
			GleanerVfxCmd.PlayOnCreature(cardPlay.Target, "res://TheGleaner/scenes/vfx/prismatic_strike_hit_vfx.tscn", 0.3f);
			await Cmd.Wait(0.3f);
		}
		AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.WithNoAttackerAnim()
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);
	}

	public override Decimal ModifyDamageAdditive(
		Creature? target,
		Decimal amount,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource) {
		if (cardSource == this && !props.HasFlag(ValueProp.Unpowered)) {
			decimal baseDamage = DynamicVars.Damage.BaseValue;
			return baseDamage * DynamicVars["Amount"].BaseValue / 100M;
		}
		return 0M;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
		CardModel card = cardPlay.Card;
		if (card != this) {
			if (card is ClusterStrike clusterStrike) {
				if (!clusterStrike.cards.Contains(this)) {
					return;
				}
			} else {
				return;
			}
		}
		card.DynamicVars["Amount"].UpgradeValueBy(card.DynamicVars["Grow"].BaseValue);
	}
	
	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
	
	public LocString getArrowName() {
		return new LocString("cards", "DEMOMOD-QUENCHED_ARROW.arrowName");
	}

	public LocString getArrowDescription() {
		return new LocString("cards", "DEMOMOD-QUENCHED_ARROW.arrowDescription");
	}

	public async Task arrowEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay, List<DamageResult> damageResults, CardModel clusterCard, AttackContext context) {
	}

	public void onMerge(CardModel clusterCard) {
		clusterCard.DynamicVars["Amount"].UpgradeValueBy(DynamicVars["Amount"].BaseValue);
	}
}
