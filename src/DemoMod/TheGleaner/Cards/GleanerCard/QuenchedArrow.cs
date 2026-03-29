using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class QuenchedArrow : CustomCardModel, IArrowCard {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar("Amount", 50),
        new DamageVar(16, ValueProp.Move)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];
    protected override HashSet<CardTag> CanonicalTags => [CustomEnums.Arrow];

    public QuenchedArrow() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        AttackCommand attackCommand = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        DynamicVars.Damage.UpgradeValueBy(DynamicVars.Damage.BaseValue * DynamicVars["Amount"].BaseValue / 100M);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4);
    
    public LocString getArrowName() {
        return new LocString("cards", "DEMOMOD-QUENCHED_ARROW.arrowName");
    }

    public LocString getArrowDescription() {
        return new LocString("cards", "DEMOMOD-QUENCHED_ARROW.arrowName");
    }

    public async Task arrowEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay, IEnumerable<DamageResult> damageResults, CardModel clusterCard) {
        clusterCard.DynamicVars.Damage.UpgradeValueBy(clusterCard.DynamicVars.Damage.BaseValue * DynamicVars["Amount"].BaseValue / 100M);
    }
}
