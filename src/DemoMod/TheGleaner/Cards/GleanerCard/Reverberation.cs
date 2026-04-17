using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Reverberation : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("GleanAmount", 1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Score)];
    public Reverberation() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await PowerCmd.Apply<ReverberationPower>(Owner.Creature, 1, Owner.Creature, this);
        await ScorePileCmd.Glean(Owner, choiceContext, DynamicVars["GleanAmount"].BaseValue, this);
    }

    protected override void OnUpgrade() {
        DynamicVars["GleanAmount"].UpgradeValueBy(1);
    }
}