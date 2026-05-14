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
using DemoMod.TheGleaner.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class ColossalBloom : CustomCardModel
{
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new PowerVar<PoisonPower>(9),
		new PowerVar<EtchPower>(3)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<EtchPower>(),
		HoverTipFactory.FromPower<PoisonPower>()
	];

	public ColossalBloom() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<PoisonPower>(Owner.Creature.CombatState.HittableEnemies, DynamicVars["PoisonPower"].BaseValue, Owner.Creature, this);
		await PowerCmd.Apply<EtchPower>(Owner.Creature.CombatState.HittableEnemies, DynamicVars["EtchPower"].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars["PoisonPower"].UpgradeValueBy(2);
		DynamicVars["EtchPower"].UpgradeValueBy(1);
	}

}
