using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class RefiningThePhrasing : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new CardsVar(1),
		new IntVar("Amount", 1)
		];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Glean)];

	
	public RefiningThePhrasing() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		if (CurrentUpgradeLevel > 0) {
			CardModel cpy = CreateClone();
			cpy.DowngradeInternal();
			await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, cpy);
			CardCmd.Preview(cpy);
		}
		await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner, false);
		await ScorePileCmd.Glean(Owner, choiceContext, DynamicVars["Amount"].BaseValue, this);
	}

	protected override void OnUpgrade() {
		AddKeyword(CardKeyword.Exhaust);
	}
}
