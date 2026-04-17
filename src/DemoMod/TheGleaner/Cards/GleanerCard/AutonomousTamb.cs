using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class AutonomousTamb : CustomCardModel, IConcertoCard {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new IntVar("KillThreshold", 12),
		new IntVar("VulVal", 2)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Concerto), HoverTipFactory.FromPower<VulnerablePower>()];

	public AutonomousTamb() : base(2, CardType.Skill, CardRarity.Common, TargetType.AllEnemies) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		foreach (Creature creature in Owner.Creature.CombatState.HittableEnemies) {
			if (creature.CurrentHp <= DynamicVars["KillThreshold"].BaseValue) {
				await CreatureCmd.Kill(creature);
			}
		}
	}

	protected override void OnUpgrade() {
		DynamicVars["KillThreshold"].UpgradeValueBy(4);
	}

	public async Task OnConcerto(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay){
	await PowerCmd.Apply<VulnerablePower>(Owner.Creature.CombatState.HittableEnemies, DynamicVars["VulVal"].BaseValue, Owner.Creature, this);
		}
	}
