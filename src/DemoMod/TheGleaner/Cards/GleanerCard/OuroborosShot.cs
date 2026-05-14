using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Cards.TokenCards;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;
[Pool(typeof(CardPool))]
public class OuroborosShot : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(11, ValueProp.Move),
		new IntVar("Amount", 1)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Glean)];

	public OuroborosShot() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) {
	}

	public override IEnumerable<CardKeyword> CanonicalKeywords =>
	[
		CardKeyword.Exhaust
	];

	private List<CardModel> optionCards;

	private List<CardModel> OptionCards {
		get {
			if (optionCards == null) {
				optionCards = [ModelDb.Card<Shuffle>().ToMutable(), ModelDb.Card<SwapPiles>().ToMutable(), ModelDb.Card<GleanCard>().ToMutable()];
				foreach (CardModel card in optionCards) {
					card.Owner = Owner;
				}
			}
			return optionCards;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);
		CardModel chosenCard = await CardSelectCmd.FromChooseACardScreen(choiceContext, OptionCards, Owner);
		if (chosenCard != null) {
			await ((IChoosable)chosenCard).OnChosen(choiceContext, cardPlay);
		}
	}

	public override async Task BeforeCombatStart() {
		if (!IsInCombat || CombatState == null || Owner.Deck.Cards.Contains(this)) {
			return;
		}

		CardCmd.Preview(this);
		await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
	}

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4);
}
