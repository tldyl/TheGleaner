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

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class BlazingHorn : CustomCardModel, IConcertoCard
{
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new CardsVar(4),
		new EnergyVar("Energy2", 2)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.ForEnergy(this),
		HoverTipFactory.FromPower<NoDrawPower>(),
		HoverTipFactory.FromKeyword(CustomEnums.Concerto)
	];

	public BlazingHorn() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner, false);
		await PowerCmd.Apply<NoDrawPower>(Owner.Creature, 1, Owner.Creature, this, false);
	}

	public async Task OnConcerto(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PlayerCmd.GainEnergy(DynamicVars["Energy2"].BaseValue, Owner);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Cards.UpgradeValueBy(2);
	}
}
