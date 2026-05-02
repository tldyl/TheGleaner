using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Godot; 
using MegaCrit.Sts2.Core.Nodes; 
using DemoMod.TheGleaner.Utils;
using MegaCrit.Sts2.Core.Commands.Builders;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class VeiledPiano : CustomCardModel, IConcertoCard
{
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(7, ValueProp.Move),
		new ExtraDamageVar(2),
		new CardsVar(1)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CustomEnums.Concerto)
	];

	public VeiledPiano() : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		Vector2 windowSize = NRun.Instance.CombatRoom.Ui.GetViewport().GetVisibleRect().Size;
		GleanerVfxCmd.PlayVfx<Node2D>(new Vector2(windowSize.X * 0.65f, windowSize.Y * 0.5f), "res://TheGleaner/scenes/vfx/aoe_attack.tscn", 0.5f);
		await CreatureCmd.TriggerAnim(Owner.Creature, "AoEAttack", 0.5f);
		AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.TargetingAllOpponents(Owner.Creature.CombatState)
			.WithNoAttackerAnim()
			.Execute(choiceContext);
	}

	public override decimal ModifyDamageAdditive(
		Creature? target,
		decimal amount,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource)
	{
		if (cardSource == this && !props.HasFlag(ValueProp.Unpowered))
		{
			return Owner.PlayerCombatState.AllCards.Count(c => c is IConcertoCard) * DynamicVars.ExtraDamage.BaseValue;
		}

		return 0M;
	}

	public async Task OnConcerto(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardCmd.PreviewCardPileAdd(
			await CardPileCmd.AddGeneratedCardToCombat(CreateClone(), PileType.Draw, true),
			2.2f
		);

		await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner, false);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(2);
		DynamicVars.ExtraDamage.UpgradeValueBy(1);
	}
}
