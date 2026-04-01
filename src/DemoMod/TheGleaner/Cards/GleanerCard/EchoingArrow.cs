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
public class EchoingArrow : CustomCardModel, IArrowCard {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(13, ValueProp.Move)
    ];
    protected override HashSet<CardTag> CanonicalTags => [CustomEnums.Arrow];

    public EchoingArrow() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        AttackCommand attackCommand = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        await arrowEffect(choiceContext, cardPlay, attackCommand.Results, this, null);
    }
    
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);

    public LocString getArrowName() {
        return new LocString("cards", "DEMOMOD-ECHOING_ARROW.arrowName");
    }

    public LocString getArrowDescription() {
        return new LocString("cards", "DEMOMOD-ECHOING_ARROW.arrowDescription");
    }

    public async Task arrowEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay, IEnumerable<DamageResult> damageResults, CardModel clusterCard, AttackContext context) {
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(clusterCard.CreateClone(), PileType.Discard, true), 2.2f);
    }
}
