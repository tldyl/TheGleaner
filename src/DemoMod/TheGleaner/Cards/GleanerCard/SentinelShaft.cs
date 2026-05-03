using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class SentinelShaft : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new BlockVar(7, ValueProp.Move)
	];

	public override bool GainsBlock => true;

	public SentinelShaft() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		List<CardPoolModel> list1 = Owner.UnlockState.CharacterCardPools.ToList();
		IEnumerable<CardModel> cards = from c in list1.SelectMany(c => c.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint))
			where c is IArrowCard
			select c;
		List<CardModel> list2 = CardFactory.GetDistinctForCombat(Owner, cards, 1, Owner.RunState.Rng.CombatCardGeneration).ToList();
		if (IsUpgraded) {
			foreach (CardModel card2 in list2) {
				CardCmd.Upgrade(card2);
			}
		}
		await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, list2[0]);
		CardCmd.Preview(list2[0]);
	}
	
	protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(1);
}
