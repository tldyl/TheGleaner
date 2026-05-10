using BaseLib.Abstracts;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

public class DirgeShuffle : CustomCardModel {
    //public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<PoisonPower>(2),
        new PowerVar<EtchPower>(1)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<PoisonPower>(),
        HoverTipFactory.FromPower<EtchPower>()
    ];

    public DirgeShuffle() : base(0, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await PowerCmd.Apply<PoisonPower>(cardPlay.Target, DynamicVars.Poison.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<EtchPower>(cardPlay.Target, DynamicVars["EtchPower"].BaseValue, Owner.Creature, this);
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
        return (PileType.Draw, CardPilePosition.Top);
    }
}
