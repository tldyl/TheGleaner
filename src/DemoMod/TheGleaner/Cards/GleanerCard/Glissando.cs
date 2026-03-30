using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Glissando : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar("Amount", 1),
        new DamageVar(5, ValueProp.Move)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    public Glissando() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AllEnemies) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", 0.5f);
        IEnumerable<DamageResult> damageResults = await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, DynamicVars.Damage, Owner.Creature,
            this);
        int count = damageResults.Count(result => result.WasTargetKilled);
        if (count == damageResults.Count() - 1) {
            await PowerCmd.Apply<WeakPower>(CombatState.HittableEnemies, DynamicVars["Amount"].BaseValue, Owner.Creature, this);
        }
    }
    
    protected override void OnUpgrade() {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars["Amount"].UpgradeValueBy(1);
    }
}
