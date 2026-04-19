using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;
using BaseLib.Patches.Content;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class EarthedBell : CustomCardModel, IConcertoCard {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(0, ValueProp.Move),
		new BlockVar(6, ValueProp.Move)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CustomEnums.Concerto)
	];
	public override bool GainsBlock => true;

	public EarthedBell() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
			.Execute(choiceContext);
	}
	
	public override Decimal ModifyDamageAdditive(
		Creature? target,
		Decimal amount,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource) {
		if (cardSource == this && !props.HasFlag(ValueProp.Unpowered)) {
			return Owner.Creature.Block;
		}
		return 0M;
	}
	
	public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side) {
	// 1. 确保只在玩家回合结束时触发
	if (side != CombatSide.Player) {
		return;
	}
	
	// 2. 获取自定义牌堆（ScorePile）的引用
	// 注意：CustomEnums.ScorePile 必须在你自己的项目枚举中已定义
	CardPile scorePile = CustomPiles.GetCustomPile(Owner.PlayerCombatState, CustomEnums.ScorePile);

	// 3. 检查卡牌是否仍在手牌中
	if (Owner.PlayerCombatState.Hand.Cards.Contains(this)) {
		// 4. 执行移动动作：将此卡(this)移入计分堆
		await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
	}
}
	
	protected override void OnUpgrade() {
		DynamicVars.Block.UpgradeValueBy(3);
	}

	public async Task OnConcerto(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
	}
}
