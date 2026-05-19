using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Utils;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class DirgeShuffle : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(1, ValueProp.Move),
		new PowerVar<PoisonPower>(2),
		new PowerVar<EtchPower>(1)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<PoisonPower>(),
		HoverTipFactory.FromPower<EtchPower>()
	];

	public DirgeShuffle() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await PowerCmd.Apply<PoisonPower>(cardPlay.Target, DynamicVars.Poison.BaseValue, Owner.Creature, this);
		await PowerCmd.Apply<EtchPower>(cardPlay.Target, DynamicVars["EtchPower"].BaseValue, Owner.Creature, this);
		AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
		.FromCard(this)            
		.Targeting(cardPlay.Target) 
		.Execute(choiceContext);    
	}

	public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(
		CardModel card,
		bool isAutoPlay,
		ResourceInfo resources,
		PileType pileType,
		CardPilePosition position) {
		if (card != this) {
			return (pileType, position);
		}
		return (IsDupe ? PileType.None : PileType.Draw, CardPilePosition.Top);
	}
	protected override void OnUpgrade()
	{
		DynamicVars["PoisonPower"].UpgradeValueBy(1);
	}
}
