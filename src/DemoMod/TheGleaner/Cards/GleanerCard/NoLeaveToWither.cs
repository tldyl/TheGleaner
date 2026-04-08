using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using DemoMod.TheGleaner.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class NoLeaveToWither : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Amount", 1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Dissonance), HoverTipFactory.FromPower<DoomPower>(), HoverTipFactory.Static(StaticHoverTip.Block)];

    public NoLeaveToWither() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await PowerCmd.Apply<NoLeaveToWitherPower>(Owner.Creature, 1, Owner.Creature, this);
        List<CardModel> cards = RandomDissonanceCard.getRandomDissonanceCards(DynamicVars["Amount"].IntValue, Owner.RunState.Rng.CombatCardGeneration);
        foreach (CardModel card in cards) {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard(card, Owner), PileType.Discard, true));
        }
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
