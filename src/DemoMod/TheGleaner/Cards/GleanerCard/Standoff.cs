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
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Standoff : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move)];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Dissonance)];

	public Standoff() : base(0, CardType.Attack, CardRarity.Common, TargetType.AllEnemies, true, true) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		IEnumerable<Creature> allEnemies = CombatState.HittableEnemies.Where(enemy => enemy.Monster.IntendsToAttack);
		await using AttackContext context = await AttackCommand.CreateContextAsync(Owner.Creature.CombatState, this);
		IEnumerable<DamageResult> damageResults = await CreatureCmd.Damage(choiceContext, allEnemies, DynamicVars.Damage, Owner.Creature, this);
		context.AddHit(damageResults);
		
		List<CardModel> cards = RandomDissonanceCard.getRandomDissonanceCards(
			1,
			Owner.RunState.Rng.CombatCardGeneration
		);

		foreach (CardModel card in cards) {
			PileType targetPile =
				Owner.RunState.Rng.CombatCardGeneration.NextInt(2) == 0
					? PileType.Draw
					: PileType.Discard;

			IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(
				[CombatState.CreateCard(card, Owner)],
				targetPile,
				true,
				CardPilePosition.Random
			);

			CardCmd.PreviewCardPileAdd(results);
		}
	}

	protected override void OnUpgrade() {
		DynamicVars.Damage.UpgradeValueBy(3);
	}
}
