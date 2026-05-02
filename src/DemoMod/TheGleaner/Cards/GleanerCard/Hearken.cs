using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Hearken : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(10, ValueProp.Move),
        new IntVar("TakeAmount", 3),
        new CardsVar(1)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(CustomEnums.Score)
    ];
    public override bool GainsBlock => true;

    public Hearken() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        CardPile drawPile = PileType.Draw.GetPile(Owner);
        if (drawPile.Cards.Count == 0) {
            return;
        }

        CardSelectorPrefs prefs = new CardSelectorPrefs(
            new LocString("cards", "DEMOMOD-HEARKEN.selectionScreenPromptDraw"),
            0,
            DynamicVars.Cards.IntValue
        );

        IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            PileType.Draw.GetPile(Owner).Cards
                .ToList().StableShuffle(Owner.RunState.Rng.CombatCardSelection).Take(DynamicVars["TakeAmount"].IntValue).ToList(),
            Owner,
            prefs
        );
        if (!selectedCards.Any()) {
            prefs = new CardSelectorPrefs(
                new LocString("cards", "DEMOMOD-HEARKEN.selectionScreenPromptDiscard"),
                DynamicVars.Cards.IntValue
            );
            selectedCards = await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                PileType.Discard.GetPile(Owner).Cards
                    .ToList().StableShuffle(Owner.RunState.Rng.CombatCardSelection).Take(DynamicVars["TakeAmount"].IntValue).ToList(),
                Owner,
                prefs
            );
        }
        foreach (CardModel selectedCard in selectedCards) {
            await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, selectedCard);
            CardCmd.Preview(selectedCard);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);
}
