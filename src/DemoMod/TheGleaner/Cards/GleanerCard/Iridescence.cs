using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using DemoMod.TheGleaner.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Iridescence : CustomCardModel {
    //public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(7, ValueProp.Move),
        new RepeatVar(7)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Score)];
    protected override bool IsPlayable => ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState).Cards.Count >= 7;
    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public Iridescence() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await CreatureCmd.TriggerAnim(Owner.Creature, "AoEAttack", 0.5f);
        AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .TargetingAllOpponents(Owner.Creature.CombatState)
            .WithNoAttackerAnim()
            .Execute(choiceContext);
        ScorePile scorePile = ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState);
        List<CardModel> discardedCards = scorePile.Cards.ToList();
        await CardCmd.Discard(choiceContext, discardedCards);
        foreach (CardModel card in discardedCards) {
            if (Owner.Creature.HasPower<StaffBurnoutPower>()) {
                await Owner.Creature.GetPower<StaffBurnoutPower>().AfterCardChangedPiles(card, CustomEnums.ScorePile, null);
            }
        }
        GleanerVfxCmd.CheckScoreIsEmpty(Owner.PlayerCombatState);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
