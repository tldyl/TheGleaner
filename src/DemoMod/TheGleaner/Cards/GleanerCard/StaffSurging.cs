using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class StaffSurging : CustomCardModel
{
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	// ✅ 数值
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new PowerVar<WeakPower>(2),
		new PowerVar<VulnerablePower>(2),
		new PowerVar<StaffSurgingPower>(0) // 默认没有
	];

	// ✅ 关键词（按你说的写法）
	public override IEnumerable<CardKeyword> CanonicalKeywords => [
		CardKeyword.Innate,
		CardKeyword.Exhaust
	];

	// ✅ HoverTip（升级后才显示第三个）
	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			yield return HoverTipFactory.FromPower<WeakPower>();
			yield return HoverTipFactory.FromPower<VulnerablePower>();

			if (DynamicVars["StaffSurgingPower"].BaseValue > 0)
			{
				yield return HoverTipFactory.FromPower<StaffSurgingPower>();
			}
		}
	}

	public StaffSurging() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
	{
	}

	// ✅ 出牌效果
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<WeakPower>(
			CombatState.HittableEnemies,
			DynamicVars["WeakPower"].BaseValue,
			Owner.Creature,
			this
		);

		await PowerCmd.Apply<VulnerablePower>(
			CombatState.HittableEnemies,
			DynamicVars["VulnerablePower"].BaseValue,
			Owner.Creature,
			this
		);

		// ✅ 只有升级后才加
		if (DynamicVars["StaffSurgingPower"].BaseValue > 0)
		{
			await PowerCmd.Apply<StaffSurgingPower>(
				CombatState.HittableEnemies,
				DynamicVars["StaffSurgingPower"].BaseValue,
				Owner.Creature,
				this
			);
		}
	}

	// ✅ 战斗开始放入乐谱
	public override async Task BeforeCombatStart()
	{
		if (!IsInCombat || CombatState == null)
		{
			return;
		}

		await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
	}

	// ✅ 升级逻辑（核心）
	protected override void OnUpgrade()
	{
DynamicVars["StaffSurgingPower"].UpgradeValueBy(1);
	}
}
