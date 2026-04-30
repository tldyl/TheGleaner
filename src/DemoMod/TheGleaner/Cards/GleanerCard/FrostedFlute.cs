using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using DemoMod.TheGleaner.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class FrostedFlute : CustomCardModel, IConcertoCard {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(10, ValueProp.Move),
		new IntVar("Amount", 2)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Concerto), HoverTipFactory.FromPower<StrengthPower>()];

	public FrostedFlute() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		SoundManager.Instance.PlaySound(SoundKeys.GetSoundResourcePath("FLUTE_" + new Random().Next(1, 5)), 1.0f);
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.WithHitFx("vfx/vfx_attack_slash")
			.TargetingAllOpponents(Owner.Creature.CombatState)
			.Execute(choiceContext);
		await PowerCmd.Apply<PreventStrengthIncreasePower>(Owner.Creature.CombatState.HittableEnemies, 2, Owner.Creature, this);
	}

	
	public async Task OnConcerto(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		SoundManager.Instance.PlaySound(SoundKeys.GetSoundResourcePath("FLUTE_" + new Random().Next(1, 5)), 1.0f);
		await PowerCmd.Apply<DemoTempLoseStrengthPower>(combatState.HittableEnemies, -DynamicVars["Amount"].BaseValue, Owner.Creature, this);
	}
	protected override void OnUpgrade() {
		DynamicVars.Damage.UpgradeValueBy(3);
		DynamicVars["Amount"].UpgradeValueBy(1);
	}
}
