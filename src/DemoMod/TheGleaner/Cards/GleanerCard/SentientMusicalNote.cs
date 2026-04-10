using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class SentientMusicalNote : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(CustomEnums.Phrase),
		HoverTipFactory.FromCard<RoundAndRound>(),
		HoverTipFactory.FromKeyword(CustomEnums.Dissonance)
	];

	public SentientMusicalNote() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self) {
		
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		await PowerCmd.Apply<SentientMusicalNotePower>(Owner.Creature, 1, Owner.Creature, this);
	}

	protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
