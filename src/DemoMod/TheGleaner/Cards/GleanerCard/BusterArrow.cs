using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class BusterArrow : CustomCardModel, IArrowCard {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(24, ValueProp.Move)
    ];
    protected override HashSet<CardTag> CanonicalTags => [CustomEnums.Arrow];

    public BusterArrow() : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        AttackContext context = await AttackCommand.CreateContextAsync(CombatState, this);
        AttackCommand attackCommand = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        context.AddHit(attackCommand.Results);
        await arrowEffect(choiceContext, cardPlay, attackCommand.Results, this, context);
    }
    
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(6);
    
    public LocString getArrowName() {
        return new LocString("cards", "DEMOMOD-BUSTER_ARROW.arrowName");
    }

    public LocString getArrowDescription() {
        return new LocString("cards", "DEMOMOD-BUSTER_ARROW.arrowDescription");
    }

    public async Task arrowEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay, IEnumerable<DamageResult> damageResults, CardModel clusterCard, AttackContext context) {
        if (damageResults.Any()) {
            DamageResult damageResult = damageResults.First();
            List<Creature> list2 = CombatState.GetTeammatesOf(damageResult.Receiver).Except<Creature>([cardPlay.Target]).Where((Func<Creature, bool>) (e => e.IsHittable)).ToList();
            if (list2.Count != 0) {
                context.AddHit(await CreatureCmd.Damage(choiceContext, list2, damageResult.TotalDamage + damageResult.OverkillDamage, ValueProp.Unpowered | ValueProp.Move, Owner.Creature, clusterCard));
            }
        }
    }
}
