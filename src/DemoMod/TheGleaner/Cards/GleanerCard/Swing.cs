using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Swing : CustomCardModel, IConcertoCard {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(2)];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CustomEnums.Concerto),
		HoverTipFactory.FromPower<StrengthPower>()
	];

	public Swing() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.RandomEnemy) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		List<CardModel?> cardsToPlay = [AutoplayRandomCardInPile(PileType.Draw), AutoplayRandomCardInPile(PileType.Hand)];
		cardsToPlay.RemoveAll(c => c == null);
		foreach (CardModel? card in cardsToPlay) {
			await CardCmd.AutoPlay(choiceContext, card, null);
		}
	}

	private CardModel? AutoplayRandomCardInPile(PileType pileType) {
		CardModel card1 = pileType.GetPile(Owner).Cards
			.Where(c => c.Type == CardType.Attack && !c.Keywords.Contains(CardKeyword.Unplayable)).ToList()
			.StableShuffle(Owner.RunState.Rng.Shuffle)
			.FirstOrDefault();
		return card1;
	}

	public async Task OnConcerto(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await PowerCmd.Apply<DemoTempStrengthPower>(Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, this);
	}
	
	protected override void OnUpgrade() {
		DynamicVars.Strength.UpgradeValueBy(1);
	}
}
