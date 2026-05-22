using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Monsters;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(StatusCardPool))]
public class Checkmate : CustomCardModel {
    public override string PortraitPath => "res://TheGleaner/images/cards/demomod-standoff.png";
    public override bool HasBuiltInOverlay => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<ProtectTheKingPower>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Retain,
        CardKeyword.Exhaust
    ];

    public override bool CanBeGeneratedInCombat => false;
    public override int MaxUpgradeLevel => 0;

    public Checkmate() : base(3, CardType.Status, CardRarity.Status, TargetType.None) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        _ = choiceContext;
        _ = cardPlay;

        List<ProtectTheKingPower> powers = Owner.Creature.CombatState.Creatures
            .Where(static creature => creature.IsAlive && creature.Monster is King)
            .Select(static creature => creature.GetPower<ProtectTheKingPower>())
            .Where(static power => power != null)
            .Cast<ProtectTheKingPower>()
            .ToList();

        foreach (ProtectTheKingPower power in powers) {
            await PowerCmd.Remove(power);
        }
    }
}
