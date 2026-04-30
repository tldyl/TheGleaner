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
		new IntVar("VigorVal", 5),
		new PowerVar<WeakPower>(2),
		new IntVar("VulVal", 2)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.Static(StaticHoverTip.Block),
		HoverTipFactory.FromKeyword(CustomEnums.Concerto),
		HoverTipFactory.FromPower<WeakPower>(),
		HoverTipFactory.FromPower<VulnerablePower>(),
		HoverTipFactory.FromPower<VigorPower>()
	];

	public Swing() : base(2, CardType.Skill, CardRarity.Common, TargetType.None)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<VigorPower>(Owner.Creature, DynamicVars["VigorVal"].BaseValue, Owner.Creature, this);

		await PowerCmd.Apply<WeakPower>(
			Owner.Creature.CombatState.HittableEnemies,
			DynamicVars["WeakPower"].BaseValue,
			Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		DynamicVars["VigorVal"].UpgradeValueBy(3);
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
