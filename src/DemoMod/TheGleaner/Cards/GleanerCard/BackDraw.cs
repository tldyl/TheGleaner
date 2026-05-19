using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class BackDraw : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4, ValueProp.Move)];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Score)];

	public BackDraw() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		CardPile discardPile = PileType.Discard.GetPile(Owner);
		List<CardModel> selectedCards = discardPile.Cards
			.Where(c => c.Tags.Contains(CardTag.Strike) || c.Tags.Contains(CustomEnums.Arrow) || c is IArrowCard)
			.ToList();
		if (selectedCards.Count == 0) {
			return;
		}
		double num2 = SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast ? 0.2 : 0.3;
		NCombatRoom instance1 = NCombatRoom.Instance;
		if (instance1 != null) {
			instance1.CombatVfxContainer.AddChildSafely((Node) NHorizontalLinesVfx.Create(new Color("FFFFFF80"), 0.8 + Mathf.Min(8, selectedCards.Count) * num2, false));
		}
		await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, selectedCards.ToArray());
		
		await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", 0.5f);
		AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.TargetingAllOpponents(Owner.Creature.CombatState)
			.WithHitCount(selectedCards.Count)
			.WithNoAttackerAnim()
			.Execute(choiceContext);
	}

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}
