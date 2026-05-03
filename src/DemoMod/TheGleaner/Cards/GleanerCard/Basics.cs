using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;
[Pool(typeof(CardPool))]
public class Basics : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new BlockVar(3, ValueProp.Move)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Score)];

	public Basics() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		DynamicVars.Block.BaseValue = Math.Max(0, DynamicVars.Block.BaseValue - 1);
		await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);

		NCard cardNode = NCombatRoom.Instance?.Ui.GetCardFromPlayContainer(this);
		if (cardNode != null) {
			Tween tween = NCombatRoom.Instance.CreateTween().SetParallel();
			tween.Parallel().TweenProperty(
				cardNode,
				(NodePath)"modulate",
				StsColors.exhaustGray,
				SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast ? 0.2 : 0.3
			);
			tween.Chain().TweenCallback(Callable.From(cardNode.QueueFreeSafely));
		}
	}

	protected override void OnUpgrade() {
		DynamicVars.Block.UpgradeValueBy(2);
	}
}
