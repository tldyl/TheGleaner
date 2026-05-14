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
public class Decompose : CustomCardModel
{
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new PowerVar<PoisonPower>(4),
		new PowerVar<EtchPower>(3),
		new PowerVar<VulnerablePower>(2),
		new PowerVar<WeakPower>(1)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<EtchPower>(),
		HoverTipFactory.FromPower<PoisonPower>()
	];

	public Decompose() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<PoisonPower>(cardPlay.Target, DynamicVars["PoisonPower"].BaseValue, Owner.Creature, this);
		await PowerCmd.Apply<EtchPower>(cardPlay.Target, DynamicVars["EtchPower"].BaseValue, Owner.Creature, this);
		await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, DynamicVars["VulnerablePower"].BaseValue, Owner.Creature, this);
		await PowerCmd.Apply<WeakPower>(cardPlay.Target, DynamicVars["WeakPower"].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars["PoisonPower"].UpgradeValueBy(1);
		DynamicVars["EtchPower"].UpgradeValueBy(1);
		DynamicVars["VulnerablePower"].UpgradeValueBy(1);
		DynamicVars["WeakPower"].UpgradeValueBy(1);
	}

}
