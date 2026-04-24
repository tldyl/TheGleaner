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
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class EchoingArrow : CustomCardModel, IArrowCard {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(12, ValueProp.Move)
	];
	protected override HashSet<CardTag> CanonicalTags => [CustomEnums.Arrow];

	public EchoingArrow() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		if (!cardPlay.IsAutoPlay) {
			GleanerVfxCmd.PlayOnCreature(cardPlay.Target, "res://TheGleaner/scenes/vfx/arrow_attack.tscn", 0.3f);
			await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", 0.5f);
			GleanerVfxCmd.PlayOnCreature(cardPlay.Target, "res://TheGleaner/scenes/vfx/arrow_hit_vfx.tscn");
		}
		AttackCommand attackCommand = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.WithNoAttackerAnim()
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);
	}
	
	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
		CardModel card = cardPlay.Card;
		if (card != this) {
			if (card is ClusterStrike clusterStrike) {
				if (!clusterStrike.cards.Contains(this)) {
					return;
				}
			} else {
				return;
			}
		}
		CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card.CreateClone(), PileType.Discard, true), 2.2f);
	}
	
	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);

	public LocString getArrowName() {
		return new LocString("cards", "DEMOMOD-ECHOING_ARROW.arrowName");
	}

	public LocString getArrowDescription() {
		return new LocString("cards", "DEMOMOD-ECHOING_ARROW.arrowDescription");
	}

	public async Task arrowEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay, List<DamageResult> damageResults, CardModel clusterCard, AttackContext context) {
	}
}
