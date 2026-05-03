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
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class StaffSurging : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Glean)];
	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Amount", 1)];

	public StaffSurging() : base(0, CardType.Skill, CardRarity.Common, TargetType.None) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		List<CardModel> cards = await ScorePileCmd.Glean(Owner, choiceContext, DynamicVars["Amount"].BaseValue, this);
		if (IsUpgraded) {
		foreach (CardModel card in cards) {
			if (card is IDissonanceCard dissonanceCard) {
				dissonanceCard.TransformFollowupAction = c => CardCmd.Upgrade(c);
			}
		}
		CardCmd.Upgrade(cards, CardPreviewStyle.HorizontalLayout);
	}
}
}
