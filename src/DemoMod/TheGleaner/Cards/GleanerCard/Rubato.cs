using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;


namespace DemoMod.TheGleaner.Cards.GleanerCard;
[Pool(typeof(CardPool))]
public class Rubato : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new EnergyVar(1),
		new EnergyVar("Energy2", 2),
		new IntVar("VulAmount", 1)
	];

	public override IEnumerable<CardKeyword> CanonicalKeywords => [
		CardKeyword.Exhaust
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Score)];

	public Rubato() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await PowerCmd.Apply<VulnerablePower>(Owner.Creature, DynamicVars["VulAmount"].BaseValue, Owner.Creature, this);
		await PowerCmd.Apply<WeakPower>(Owner.Creature, DynamicVars["VulAmount"].BaseValue, Owner.Creature, this);
		await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
		if (CurrentUpgradeLevel > 0) {
			CardModel cpy = CreateClone();
			cpy.DowngradeInternal();
			await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, cpy);
			CardCmd.Preview(cpy);
		}
	}

	public override async Task BeforeCombatStart() {
		if (!IsInCombat || CombatState == null || Owner.Deck.Cards.Contains(this)) {
			return;
		}

		CardCmd.Preview(this);
		await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
	}
}
