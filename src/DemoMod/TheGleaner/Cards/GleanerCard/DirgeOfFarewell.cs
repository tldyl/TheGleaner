using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

using MegaCrit.Sts2.Core.Models.CardPools;
namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(StatusCardPool))]
public class DirgeOfFarewell : CustomCardModel, IDissonanceCard {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(12, ValueProp.Unpowered)
	];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<PulsationOfTheTides>()];
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal, CardKeyword.Exhaust, CustomEnums.Dissonance];

	public DirgeOfFarewell() : base(1, CardType.Status, CardRarity.Status, TargetType.AllEnemies) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
	}

	public override async Task AfterCardExhausted(
		PlayerChoiceContext choiceContext,
		CardModel card,
		bool causedByEthereal) {
		await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", 0.5f);
		if (card == this) {
			IEnumerable<DamageResult> _ = await CreatureCmd.Damage(choiceContext, CombatState.Creatures, DynamicVars.Damage, Owner.Creature,
				this);
		}
	}

	public void OnEnterScorePile(PlayerCombatState combatState, Player player) {
		CardModel card = Owner.Creature.CombatState.CreateCard<PulsationOfTheTides>(Owner);
		TransformFollowupAction?.Invoke(card);
		TaskHelper.RunSafely(CardCmd.Transform(this, card));
	}

	public Action<CardModel> TransformFollowupAction { get; set; }
}
