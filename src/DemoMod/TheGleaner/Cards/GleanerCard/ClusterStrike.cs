using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(TokenCardPool))]
public class ClusterStrike : CustomCardModel, IAppendDescriptionCard {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<IHoverTip> ExtraHoverTips {
        get {
            return cards.Where(card => card is IArrowCard).Select(card => {
                IArrowCard arrowCard = card as IArrowCard;
                return (IHoverTip) new HoverTip(arrowCard.getArrowName(), arrowCard.getArrowDescription());
            });
        }
    }
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar("Amount", 0),
        new IntVar("HitCount", 0),
        new IntVar("Grow", 50),
        new DamageVar(6, ValueProp.Move)
    ];
    private List<CardModel> cards = [];

    public ClusterStrike() : base(1, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) {
    }

    public void setCards(List<CardModel> cards) {
        int hitCount = 0;
        foreach (CardModel card in cards) {
            if (!this.cards.Any(c => c.Id.Equals(card.Id))) {
                this.cards.Add(card);
            }
            if (card is IArrowCard arrowCard) {
                arrowCard.onMerge(this);
            }
            if (card is ClusterStrike) {
                hitCount += card.DynamicVars["HitCount"].IntValue;
            } else {
                hitCount++;
            }
        }

        DynamicVars["HitCount"].UpgradeValueBy(hitCount);
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        AttackContext context = await AttackCommand.CreateContextAsync(CombatState, this);
        AttackCommand attackCommand = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .WithHitCount(DynamicVars["HitCount"].IntValue)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        context.AddHit(attackCommand.Results);
        foreach (CardModel card in cards) {
            if (card is IArrowCard arrowCard) {
                await arrowCard.arrowEffect(choiceContext, cardPlay, attackCommand.Results, this, context);
            }
        }
    }

    public override Decimal ModifyDamageAdditive(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource) {
        if (cardSource == this && !props.HasFlag(ValueProp.Unpowered)) {
            decimal baseDamage = DynamicVars.Damage.BaseValue;
            return baseDamage * DynamicVars["Amount"].BaseValue / 100M;
        }
        return 0M;
    }
    
    protected override void OnUpgrade() {
        DynamicVars.Damage.UpgradeValueBy(3);
    }

    public string AppendDescription() {
        List<string> descriptions = [];
        descriptions.AddRange(cards.Where(card => card is IArrowCard).Select(card => ((IArrowCard) card).getArrowName().GetFormattedText()));
        return "[gold]" + string.Join<string>("[/gold]" + '\n' + "[gold]", descriptions) + "[/gold]";
    }
}
