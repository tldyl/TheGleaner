using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using DemoMod.TheGleaner.Actions;
using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace DemoMod.TheGleaner.Cards.GleanerCard;
[Pool(typeof(CardPool))]
public class ScoreEntryCard : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Score)];
    protected override bool ShouldGlowGoldInternal {
        get {
            ScorePile scorePile = ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState);
            return scorePile.freeTakeCount > 0;
        }
    }

    protected override bool ShouldGlowRedInternal => Owner.Creature.HasPower<StaffBurnoutPower>() || ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState).freeTakeCount == 0;
    
    public ScoreEntryCard() : base(0, CardType.Status, CardRarity.Event, TargetType.Self) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await Cmd.Wait(0.2f);
        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new TakeCardsFromScoreAction(Owner));
    }

    protected override PileType GetResultPileType() {
        PileType resultPileType = base.GetResultPileType();
        return resultPileType != PileType.Discard ? resultPileType : decidePile();
    }

    private PileType decidePile() {
        ScorePile scorePile = (ScorePile)CustomPiles.GetCustomPile(Owner.PlayerCombatState, CustomEnums.ScorePile);
        return scorePile.Cards.Count == 0 ? PileType.None : PileType.Hand;
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card) {
        if (card == this) {
            await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Top, skipVisuals: true);
        }
    }
}
