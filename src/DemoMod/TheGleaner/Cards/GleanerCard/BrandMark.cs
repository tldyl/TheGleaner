using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class BrandMark : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(6, ValueProp.Move),
		new IntVar("Times", 6),
		new IntVar("BlockGained", 2)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];

	public BrandMark() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);
		await PowerCmd.Apply<BrandMarkPower>(cardPlay.Target, DynamicVars["Times"].BaseValue, Owner.Creature, this);
	}
		protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(2);
		DynamicVars["Times"].UpgradeValueBy(3);
	}

}
