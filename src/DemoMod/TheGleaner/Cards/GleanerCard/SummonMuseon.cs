using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class SummonMuseon : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(10, ValueProp.Move),
		new EnergyVar(1)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this), HoverTipFactory.FromKeyword(CustomEnums.Score)];

	protected override bool ShouldGlowGoldInternal {
		get {
			ScorePile scorePile = ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState);
			return scorePile.cardsAddedToScoreThisTurn;
		}
	}
	
	public SummonMuseon() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		if (!cardPlay.IsAutoPlay) {
			GleanerVfxCmd.PlayOnCreature<Node2D>(cardPlay.Target, "res://TheGleaner/scenes/vfx/arrow_attack.tscn", 0.3f);
			await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", 0.5f);
			GleanerVfxCmd.PlayOnCreature<Node2D>(cardPlay.Target, "res://TheGleaner/scenes/vfx/arrow_hit_vfx.tscn");
		}
		AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.WithNoAttackerAnim()
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);
		if (ShouldGlowGoldInternal) {
			await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
		}
	}

	protected override void OnUpgrade() {
		DynamicVars.Damage.UpgradeValueBy(3);
		DynamicVars.Energy.UpgradeValueBy(1);
	}
}
