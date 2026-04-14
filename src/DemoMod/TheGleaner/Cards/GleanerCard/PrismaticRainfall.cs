using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class PrismaticRainfall : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(7, ValueProp.Move)
    ];

    public PrismaticRainfall() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        AttackContext context = await AttackCommand.CreateContextAsync(Owner.Creature.CombatState, this);
        await using (context) {
            IEnumerable<DamageResult> damageResults = await CreatureCmd.Damage(choiceContext, Owner.Creature.CombatState.HittableEnemies, DynamicVars.Damage, Owner.Creature, this);
            context.AddHit(damageResults);
        }
        List<CardModel> list = [];
        list.AddRange(PileType.Draw.GetPile(Owner).Cards.Where(c => c.Tags.Contains(CardTag.Strike) || c is IArrowCard));
        list.AddRange(PileType.Hand.GetPile(Owner).Cards.Where(c => c.Tags.Contains(CardTag.Strike) || c is IArrowCard));
        list.AddRange(PileType.Discard.GetPile(Owner).Cards.Where(c => c.Tags.Contains(CardTag.Strike) || c is IArrowCard));
        list.AddRange(ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState).Cards.Where(c => c.Tags.Contains(CardTag.Strike) || c is IArrowCard));
        bool flag = true;
        foreach (CardModel card in list) {
            await CardCmd.AutoPlay(choiceContext, card, null, skipCardPileVisuals: !flag);
            flag = false;
        }
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
