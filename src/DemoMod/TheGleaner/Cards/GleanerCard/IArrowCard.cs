using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

public interface IArrowCard {
	public LocString getArrowName();

	public LocString getArrowDescription();

	public Task arrowEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay, List<DamageResult> damageResults, CardModel clusterCard, AttackContext context);

	public void onMerge(CardModel clusterCard) {
		
	}
}
