using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Boiling : CustomCardModel
{
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new EnergyVar("EnergyAmount", 3),
		new EnergyVar("Amount", 1)
	];

	public Boiling() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PlayerCmd.GainEnergy(DynamicVars["EnergyAmount"].BaseValue, Owner);

		List<CardModel> handCards = PileType.Hand.GetPile(Owner).Cards.ToList();
		foreach (CardModel card in handCards)
		{
			card.EnergyCost.AddUntilPlayed((int)DynamicVars["Amount"].BaseValue, false);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars["EnergyAmount"].UpgradeValueBy(1);
	}
}
