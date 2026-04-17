using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using MegaCrit.Sts2.Core.HoverTips;


namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Rubato : CustomCardModel
{
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new EnergyVar(1),
		new EnergyVar("Energy2", 2)
	];

	public override IEnumerable<CardKeyword> CanonicalKeywords => [
		CardKeyword.Exhaust
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Score)];

	public Rubato() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
	}

	public override async Task BeforeCombatStart()
{
	if (!IsInCombat || CombatState == null) {
		return;
	}

	await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
}   

	protected override void OnUpgrade()
	{
		DynamicVars.Energy.UpgradeValueBy(1);
	}
}
