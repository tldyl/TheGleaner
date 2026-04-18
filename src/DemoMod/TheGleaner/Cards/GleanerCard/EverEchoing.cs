using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class EverEchoing : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [new RepeatVar(1)];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.ReplayStatic), HoverTipFactory.FromKeyword(CustomEnums.Score)];

	public EverEchoing() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		ScorePile scorePile = ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState);
		CardSelectorPrefs prefs = new CardSelectorPrefs(new LocString("cards", "DEMOMOD-EVER_ECHOING.selectionScreenPrompt"), 1);
		CardModel selectedCard = (await CardSelectCmd.FromSimpleGrid(choiceContext, scorePile.Cards.Where(c => c.Type == CardType.Attack).ToList(), Owner, prefs)).FirstOrDefault();
		if (selectedCard != null) {
			selectedCard.BaseReplayCount += DynamicVars.Repeat.IntValue;
			if (!selectedCard.Keywords.Contains(CardKeyword.Exhaust)) {
				selectedCard.AddKeyword(CardKeyword.Exhaust);
			}
		}
	}
	
	protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
