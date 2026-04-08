using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
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
public class SwiftArrow : CustomCardModel, IArrowCard {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar("Amount", 1),
        new DamageVar(4, ValueProp.Move)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.Static(StaticHoverTip.Block)];
    protected override HashSet<CardTag> CanonicalTags => [CustomEnums.Arrow];

    public SwiftArrow() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        AttackCommand attackCommand = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        foreach (DamageResult damageResult in attackCommand.Results) {
            if (damageResult.Receiver == cardPlay.Target && !damageResult.WasFullyBlocked) {
                await PowerCmd.Apply<SwiftArrowPower>(cardPlay.Target, DynamicVars["Amount"].BaseValue, Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
    
    public LocString getArrowName() {
        return new LocString("cards", "DEMOMOD-SWIFT_ARROW.arrowName");
    }

    public LocString getArrowDescription() {
        return new LocString("cards", "DEMOMOD-SWIFT_ARROW.arrowDescription");
    }

    public async Task arrowEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay, IEnumerable<DamageResult> damageResults, CardModel clusterCard, AttackContext context) {
        foreach (DamageResult damageResult in damageResults) {
            if (damageResult.Receiver == cardPlay.Target && !damageResult.WasFullyBlocked) {
                await PowerCmd.Apply<SwiftArrowPower>(cardPlay.Target, DynamicVars["Amount"].BaseValue, Owner.Creature, this);
            }
        }
    }
}
