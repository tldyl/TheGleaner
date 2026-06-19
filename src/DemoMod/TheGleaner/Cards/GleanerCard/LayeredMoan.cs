using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class LayeredMoan : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(5, ValueProp.Move),
		new CardsVar(1)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<PoisonPower>(),
		HoverTipFactory.FromKeyword(CustomEnums.Dissonance),
		HoverTipFactory.FromCard<DirgeOfFarewell>(),
		HoverTipFactory.FromCard<ShriekOfDread>(),
		HoverTipFactory.FromCard<HowlOfWrath>()
	];

	public LayeredMoan() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);
		if (cardPlay.Target != null) {
			List<DamageResult> damageResults = [];
			foreach (List<DamageResult> results in _.Results) {
				damageResults.AddRange(results);
			}
			await PowerCmd.Apply<PoisonPower>(choiceContext, cardPlay.Target, damageResults.FirstOrDefault().TotalDamage, Owner.Creature, this);
		}
		List<CardModel> cards = RandomDissonanceCard.getRandomDissonanceCards(
			DynamicVars.Cards.IntValue,
			Owner.RunState.Rng.CombatCardGeneration
		);

		SoundManager.Instance.PlaySound(SoundKeys.HEART_BEAT);
		foreach (CardModel card in cards) {
			PileType targetPile =
				Owner.RunState.Rng.CombatCardGeneration.NextInt(2) == 0
					? PileType.Draw
					: PileType.Discard;

			CardCmd.PreviewCardPileAdd(
				await CardPileCmd.AddGeneratedCardToCombat(
					CombatState.CreateCard(card, Owner),
					targetPile,
					Owner
				)
			);
		}
	}
	
	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}
