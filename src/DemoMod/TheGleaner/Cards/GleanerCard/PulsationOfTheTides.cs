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
public class PulsationOfTheTides : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move),
        new IntVar("StrMul", 2)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public PulsationOfTheTides() : base(1, CardType.Attack, CardRarity.Event, TargetType.AllEnemies) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", 0.5f);
        IEnumerable<DamageResult> damageResults = await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, DynamicVars.Damage, Owner.Creature,
            this);
        int count = damageResults.Count(result => result.WasTargetKilled);
        if (count == damageResults.Count() - 1) {
            IEnumerable<DamageResult> _ = await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, DynamicVars.Damage, Owner.Creature,
                this);
        }
    }
    
    public override Decimal ModifyDamageAdditive(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource) {
        if (cardSource == this && !props.HasFlag(ValueProp.Unpowered)) {
            int strAmount = 0;
            if (dealer != null && dealer.Powers.Any(p => p is StrengthPower)) {
                StrengthPower strengthPower = dealer.Powers.First(p => p is StrengthPower) as StrengthPower;
                strAmount = strengthPower.Amount;
            }
            return strAmount * (DynamicVars["StrMul"].BaseValue - 1M);
        }
        return 0M;
    }

    protected override void OnUpgrade() => DynamicVars["StrMul"].UpgradeValueBy(2);
}
