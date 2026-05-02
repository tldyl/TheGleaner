using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Cards.TokenCards;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class SonoweaveFlash : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new CardsVar(1),
		new DamageVar(13, ValueProp.Move)
	];
	
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Score)];
	private List<CardModel> OptionCards {
		get {
			List<CardModel> optionCards = [
				ModelDb.Card<ZeroCostAttacks>().ToMutable(),
				ModelDb.Card<OneCostAttacks>().ToMutable(),
				ModelDb.Card<TwoCostAttacks>().ToMutable(),
				ModelDb.Card<ThreeOrMoreCostAttacks>().ToMutable()
			];
			foreach (CardModel card in optionCards) {
				card.Owner = Owner;
				card.DynamicVars.Cards.BaseValue = DynamicVars.Cards.BaseValue;
				if (IsUpgraded) {
					CardCmd.Upgrade(card);
				}
			}
			return optionCards;
		}
	}

	public SonoweaveFlash() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);
		CardModel chosenCard = await CardSelectCmd.FromChooseACardScreen(choiceContext, OptionCards, Owner);
		if (chosenCard != null) {
			await ((IChoosable) chosenCard).OnChosen(choiceContext, cardPlay, 1);
		}
	}
	
	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}
