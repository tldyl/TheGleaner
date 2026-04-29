using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class ThreshersAxe : CustomCardModel, IConcertoCard {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new IntVar("Amount", 4),
		new DamageVar(20, ValueProp.Move)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.FromKeyword(CustomEnums.Concerto), HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];
	
	public ThreshersAxe() : base(3, CardType.Attack, CardRarity.Rare, TargetType.RandomEnemy) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.TargetingRandomOpponents(Owner.Creature.CombatState)
			.Execute(choiceContext);
		if (Owner.Creature.CombatState.HittableEnemies.Count == 0 && cardPlay.IsAutoPlay) {
			await CombatManager.Instance.CheckWinCondition();
		}
	}

	public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(
		CardModel card,
		bool isAutoPlay,
		ResourceInfo resources,
		PileType pileType,
		CardPilePosition position) {
		return card != this ? (pileType, position) : (PileType.Hand, position);
	}
	
	public override Decimal ModifyDamageAdditive(
		Creature? target,
		Decimal amount,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource) {
		if (cardSource == this && !props.HasFlag(ValueProp.Unpowered)) {
			int strAmount = 0;
			if (dealer != null && dealer.Powers.Any(p => p is StrengthPower)) {
				StrengthPower strengthPower = dealer.Powers.First(p => p is StrengthPower) as StrengthPower;
				strAmount = strengthPower.Amount;
			}
			return strAmount * (DynamicVars["Amount"].BaseValue - 1M);
		}
		return 0M;
	}
	
	protected override void OnUpgrade() => DynamicVars["Amount"].UpgradeValueBy(3);

	public async Task OnConcerto(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await CardCmd.AutoPlay(choiceContext, this, null);
		AddKeyword(CardKeyword.Ethereal);
	}
}
