using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Nodes.Vfx;
using DemoMod.TheGleaner.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(TokenCardPool))]
public class ClusterStrike : CustomCardModel, IAppendDescriptionCard {
	public override string PortraitPath => GetPortraitPath();

	protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

	protected override IEnumerable<IHoverTip> ExtraHoverTips {
		get {
			return cards.Where(card => card is IArrowCard).Select(card =>
			{
				IArrowCard arrowCard = (IArrowCard)card;
				return (IHoverTip)new HoverTip(arrowCard.getArrowName(), arrowCard.getArrowDescription());
			});
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new IntVar("Amount", 0),
		new IntVar("HitCount", 0),
		new IntVar("Grow", 50),
		new DamageVar(6, ValueProp.Move)
	];

	public List<CardModel> cards = [];

	public ClusterStrike() : base(2, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) {
	}

	private string GetPortraitPath() {
		int hitCount = GetCurrentHitCount();
		int stage = GetPortraitStage(hitCount);
		return $"res://TheGleaner/images/cards/demomod-cluster_strike_{stage}.png";
	}

	private int GetCurrentHitCount() {
		if (DynamicVars == null || !DynamicVars.ContainsKey("HitCount")) {
			return 0;
		}

		return DynamicVars["HitCount"].IntValue;
	}

	private int GetPortraitStage(int hitCount) {
		// strike_1 对应 1 hit 和 2 hit
		// strike_2 对应 3 hit
		// strike_3 对应 4 hit
		// ...
		// strike_9 对应 10 hit 及以上
		return Math.Min(Math.Max(hitCount - 1, 1), 9);
	}

	public void setCards(List<CardModel> cards) {
		int hitCount = 0;
		this.cards.Clear();

		foreach (CardModel card in cards) {
			if (!this.cards.Any(c => c.Id.Equals(card.Id))) {
				this.cards.Add(card);
			}

			if (card is IArrowCard arrowCard) {
				arrowCard.onMerge(this);
			}

			if (card is ClusterStrike clusterStrike) {
				hitCount += card.DynamicVars["HitCount"].IntValue;

				if (card.IsUpgraded && !IsUpgraded) {
					UpgradeInternal();
					FinalizeUpgradeInternal();
				}

				foreach (CardModel card1 in clusterStrike.cards) {
					if (!this.cards.Any(c => c.Id.Equals(card1.Id))) {
						this.cards.Add(card1);
					}

					if (card1 is IArrowCard arrowCard1) {
						arrowCard1.onMerge(this);
					}
				}
			} else {
				hitCount++;
			}
		}

		foreach (CardModel card in this.cards.Where(c => c is BusterArrow).ToList()) {
			this.cards.Remove(card);
			this.cards.Insert(0, card);
		}

		DynamicVars["HitCount"].UpgradeValueBy(hitCount);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await using AttackContext context = await AttackCommand.CreateContextAsync(Owner.Creature.CombatState, this);

		List<string> sfxGroup = HitSfxGroup.nextGroup("harp", DynamicVars["HitCount"].IntValue);

		NClusterStrikeVfxOrb orbVfx = GleanerVfxCmd.PlayOnCreature<NClusterStrikeVfxOrb>(cardPlay.Target, "res://TheGleaner/scenes/vfx/cluster_strike_vfx/orb.tscn");
		await Cmd.Wait(0.6f);
		for (int i = 0; i < DynamicVars["HitCount"].IntValue; i++) {
			GleanerVfxCmd.PlayOnCreature<Node2D>(cardPlay.Target, $"res://TheGleaner/scenes/vfx/cluster_strike_vfx/arrow_{GD.RandRange(1, 3)}.tscn");
			await Cmd.Wait(0.0667f);
			IEnumerable<DamageResult> damageResults = await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage, this);
			context.AddHit(damageResults);
			SfxCmd.Play(sfxGroup[0]);
			sfxGroup.RemoveAt(0);

			List<DamageResult> damageResultsList = damageResults.ToList();
			foreach (CardModel card in cards) {
				if (card is IArrowCard arrowCard) {
					await arrowCard.arrowEffect(choiceContext, cardPlay, damageResultsList, this, context);
				}
			}
			if (!cardPlay.Target.IsAlive) {
				break;
			}
		}
		orbVfx.end();
	}

	public override decimal ModifyDamageAdditive(
		Creature? target,
		decimal amount,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource) {
		if (cardSource == this && !props.HasFlag(ValueProp.Unpowered)) {
			decimal baseDamage = DynamicVars.Damage.BaseValue;
			return baseDamage * DynamicVars["Amount"].BaseValue / 100M;
		}

		return 0M;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
		foreach (CardModel cardModel in cards) {
			if (cardModel is ClusterStrike) {
				continue;
			}
			await cardModel.AfterCardPlayed(context, cardPlay);
		}
	}

	protected override void OnUpgrade() {
		DynamicVars.Damage.UpgradeValueBy(3);
	}

	public string AppendDescription() {
		List<string> descriptions = [];
		descriptions.AddRange(cards
			.Where(card => card is IArrowCard)
			.Select(card => "[gold]" + ((IArrowCard)card).getArrowName().GetFormattedText() + "[/gold]" + new LocString("cards", "DEMOMOD-CLUSTER_STRIKE.period").GetFormattedText() + "\n"));

		return string.Join("", descriptions);
	}
}
