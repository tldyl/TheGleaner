using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Swing : CustomCardModel, IConcertoCard
{
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new BlockVar(10, ValueProp.Move),
		new PowerVar<WeakPower>(1),
		new IntVar("VulVal", 1)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.Static(StaticHoverTip.Block),
		HoverTipFactory.FromKeyword(CustomEnums.Concerto),
		HoverTipFactory.FromPower<WeakPower>(),
		HoverTipFactory.FromPower<VulnerablePower>()
	];

	public Swing() : base(2, CardType.Skill, CardRarity.Common, TargetType.None)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

		await PowerCmd.Apply<WeakPower>(
			Owner.Creature.CombatState.HittableEnemies,
			DynamicVars["WeakPower"].BaseValue,
			Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		DynamicVars["WeakPower"].UpgradeValueBy(1);
		DynamicVars["VulVal"].UpgradeValueBy(1);
	}

	public async Task OnConcerto(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<VulnerablePower>(
			Owner.Creature.CombatState.HittableEnemies,
			DynamicVars["VulVal"].BaseValue,
			Owner.Creature,
			this
		);
	}
}
