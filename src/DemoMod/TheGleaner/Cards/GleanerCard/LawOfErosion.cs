using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class LawOfErosion : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(0, ValueProp.Move),
        new PowerVar<VulnerablePower>(1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Block),
        HoverTipFactory.FromPower<SlipperyPower>(),
        HoverTipFactory.FromPower<IntangiblePower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override bool GainsBlock => true;

    public LawOfErosion() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await CreatureCmd.LoseBlock(cardPlay.Target, cardPlay.Target.Block);
        if (cardPlay.Target.HasPower<SlipperyPower>()) {
            await PowerCmd.Remove<SlipperyPower>(cardPlay.Target);
        }
        if (cardPlay.Target.HasPower<IntangiblePower>()) {
            await PowerCmd.Remove<IntangiblePower>(cardPlay.Target);
        }
        await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, DynamicVars["VulnerablePower"].BaseValue, Owner.Creature, this);
    }

    public override Decimal ModifyBlockAdditive(Creature target,
        Decimal block,
        ValueProp props,
        CardModel? cardSource,
        CardPlay? cardPlay) {
        if (cardPlay.Target != null && cardPlay.Card == this) {
            int artifactAmount = 0;
            if (cardPlay.Target.HasPower<SlipperyPower>()) {
                artifactAmount += cardPlay.Target.GetPower<SlipperyPower>().Amount;
            }
            if (cardPlay.Target.HasPower<IntangiblePower>()) {
                artifactAmount += cardPlay.Target.GetPower<IntangiblePower>().Amount;
            }
            return cardPlay.Target.Block + artifactAmount;
        }
        return 0;
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
