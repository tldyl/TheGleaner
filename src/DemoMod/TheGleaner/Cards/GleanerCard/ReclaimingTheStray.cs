using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class ReclaimingTheStray : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(4, ValueProp.Move)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public ReclaimingTheStray() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        CardPile discardPile = PileType.Discard.GetPile(Owner);
        if (discardPile.Cards.Count == 0) {
            return;
        }

        CardSelectorPrefs prefs = new CardSelectorPrefs(new LocString("cards", "DEMOMOD-RECLAIMING_THE_STRAY.selectionScreenPrompt"), 1);
        CardModel selectedCard = (await CardSelectCmd.FromSimpleGrid(choiceContext, discardPile.Cards, Owner, prefs)).FirstOrDefault();
        if (selectedCard != null) {
            await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, selectedCard);
        }
    }

    protected override void OnUpgrade() {
        DynamicVars.Block.UpgradeValueBy(2);
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
