using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class SummonMuseon : CustomCardModel {
    //public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5, ValueProp.Move),
        new CardsVar(1)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Score)];

    public SummonMuseon() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        CardPile discardPile = PileType.Discard.GetPile(Owner);
        if (discardPile.Cards.Count == 0) {
            return;
        }

        CardSelectorPrefs prefs = new CardSelectorPrefs(
            new LocString("cards", "DEMOMOD-RECLAIMING_THE_STRAY.selectionScreenPrompt"),
            DynamicVars.Cards.IntValue
        );

        IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            discardPile.Cards,
            Owner,
            prefs
        );
        foreach (CardModel selectedCard in selectedCards) {
            await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, selectedCard);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}
