using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class RefiningThePhrasing : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new IntVar("Amount", 1)
	];
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Glean), HoverTipFactory.FromKeyword(CustomEnums.Score)];

	public RefiningThePhrasing() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		List<CardModel> selectedCards = (await ScorePileCmd.ShowScorePileScreen(Owner.PlayerCombatState, choiceContext, Owner, true)).ToList();
		if (selectedCards.Count > 0) {
			int gleanAmount = selectedCards.Count + (CurrentUpgradeLevel > 0 ? 1 : 0);
			await CardCmd.Discard(choiceContext, selectedCards);
			foreach (CardModel card in selectedCards) {
				if (Owner.Creature.HasPower<StaffBurnoutPower>()) {
					await Owner.Creature.GetPower<StaffBurnoutPower>().AfterCardChangedPiles(card, CustomEnums.ScorePile, null);
				}
			}
			await ScorePileCmd.Glean(Owner, choiceContext, gleanAmount, this);
		} else {
			await ScorePileCmd.Glean(Owner, choiceContext, DynamicVars["Amount"].BaseValue, this);
		}
	}
	
	protected override void OnUpgrade() => DynamicVars["Amount"].UpgradeValueBy(1);
}
