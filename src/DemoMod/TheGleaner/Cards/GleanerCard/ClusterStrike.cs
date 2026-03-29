using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
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
        new DamageVar(6, ValueProp.Move)
    ];
    private List<CardModel> cards = [];

    public ClusterStrike() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy) {
    }

    public void setCards(List<CardModel> cards) {
        this.cards.AddRange(cards);
        DynamicVars["Amount"].UpgradeValueBy(cards.Count);
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        AttackCommand attackCommand = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .WithHitCount(DynamicVars["Amount"].IntValue)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        foreach (CardModel card in cards) {
            if (card is IArrowCard arrowCard) {
                await arrowCard.arrowEffect(choiceContext, cardPlay, attackCommand.Results, this);
            }
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);

    public string AppendDescription() {
        List<string> descriptions = [];
        descriptions.AddRange(cards.Where(card => card is IArrowCard).Select(card => ((IArrowCard) card).getArrowName().GetFormattedText()));
        return "[gold]" + string.Join<string>("[/gold]" + '\n' + "[gold]", descriptions) + "[/gold]";
    }
}
