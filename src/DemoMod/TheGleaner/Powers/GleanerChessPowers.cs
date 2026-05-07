using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Powers;

public sealed class GleanerPromotionPower : CustomPowerModel
{
	private const string PowerIconPath = "res://TheGleaner/images/powers/demomod-staff_burnout_power.png";

	public override string CustomPackedIconPath => PowerIconPath;

	public override string CustomBigIconPath => PowerIconPath;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public void TriggerFlash()
	{
		Flash();
	}
}

public sealed class GleanerWhiteSquareDomainPower : CustomPowerModel
{
	private const string PowerIconPath = "res://TheGleaner/images/powers/demomod-the_lake_mirror_power.png";

	public override string CustomPackedIconPath => PowerIconPath;

	public override string CustomBigIconPath => PowerIconPath;

	public override LocString Title => new("powers", "gleanerWhiteSquareDomainPower.title");

	public override LocString Description => new("powers", "gleanerWhiteSquareDomainPower.description");

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override string SmartDescriptionLocKey => "gleanerWhiteSquareDomainPower.smartDescription";

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<StrengthPower>()
	];

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner.Creature?.IsPlayer != true || cardPlay.Card.Type != CardType.Skill)
		{
			return;
		}

		Flash();
		await PowerCmd.Apply<StrengthPower>(Owner, Amount, Owner, null);
		await PowerCmd.Apply<GleanerStrengthDecayPower>(Owner, Amount, Owner, null);
	}
}

public sealed class GleanerBlackSquareDomainPower : CustomPowerModel
{
	private const string PowerIconPath = "res://TheGleaner/images/powers/demomod-one_winged_violin_power.png";

	public override string CustomPackedIconPath => PowerIconPath;

	public override string CustomBigIconPath => PowerIconPath;

	public override LocString Title => new("powers", "gleanerBlackSquareDomainPower.title");

	public override LocString Description => new("powers", "gleanerBlackSquareDomainPower.description");

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override string SmartDescriptionLocKey => "gleanerBlackSquareDomainPower.smartDescription";

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<StrengthPower>()
	];

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner.Creature?.IsPlayer != true || cardPlay.Card.Type != CardType.Attack)
		{
			return;
		}

		Flash();
		await PowerCmd.Apply<StrengthPower>(Owner, Amount, Owner, null);
		await PowerCmd.Apply<GleanerStrengthDecayPower>(Owner, Amount, Owner, null);
	}
}

public sealed class GleanerFanaticPower : CustomPowerModel
{
	private const string PowerIconPath = "res://TheGleaner/images/powers/demomod-rendezvous_with_doom_power.png";

	public override string CustomPackedIconPath => PowerIconPath;

	public override string CustomBigIconPath => PowerIconPath;

	public override LocString Title => new("powers", "gleanerFanaticPower.title");

	public override LocString Description => new("powers", "gleanerFanaticPower.description");

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override string SmartDescriptionLocKey => "gleanerFanaticPower.smartDescription";

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<GleanerStrengthDecayPower>()
	];

	public override async Task AfterDamageReceived(
		PlayerChoiceContext choiceContext,
		Creature target,
		DamageResult result,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource)
	{
		_ = choiceContext;
		_ = props;
		_ = dealer;
		if (target != Owner || result.UnblockedDamage <= 0 || cardSource?.Type != CardType.Attack)
		{
			return;
		}

		GleanerStrengthDecayPower? decay = Owner.GetPower<GleanerStrengthDecayPower>();
		if (decay == null)
		{
			return;
		}

		Flash();
		await PowerCmd.Remove(decay);
	}
}

public sealed class GleanerStrengthDecayPower : CustomPowerModel
{
	private const string PowerIconPath = "res://TheGleaner/images/powers/demomod-demo_temp_strength_power.png";

	public override string CustomPackedIconPath => PowerIconPath;

	public override string CustomBigIconPath => PowerIconPath;

	public override LocString Title => new("powers", "gleanerStrengthDecayPower.title");

	public override LocString Description => new("powers", "gleanerStrengthDecayPower.description");

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override string SmartDescriptionLocKey => "gleanerStrengthDecayPower.smartDescription";

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<StrengthPower>()
	];

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		_ = choiceContext;
		if (side != Owner.Side)
		{
			return;
		}

		Flash();
		int amount = Amount;
		await PowerCmd.Remove(this);
		await PowerCmd.Apply<StrengthPower>(Owner, -amount, Owner, null);
	}
}
