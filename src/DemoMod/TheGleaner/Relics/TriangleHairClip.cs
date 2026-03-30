using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace DemoMod.TheGleaner.Relics;

[Pool(typeof(JeraRelicPool))]
public sealed class TriangleHairClip : JeraExclusiveRelic {
    private const string AmountKey = "Amount";
    private bool _pendingMoveFromDrawPile;

    public override RelicRarity Rarity => RelicRarity.Common;
    public override string PackedIconPath => "res://TheGleaner/images/relics/demomod-triangle_hair_clip.png";
    protected override string PackedIconOutlinePath => "res://TheGleaner/images/relics/demomod-triangle_hair_clip_outline.png";
    protected override string BigIconPath => "res://TheGleaner/images/relics/demomod-triangle_hair_clip.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new List<DynamicVar> { new IntVar(AmountKey, 2) };

    public override async Task BeforeCombatStart() {
        if (Owner == null || !IsOwnerJera()) {
            return;
        }

        ScorePileCmd.InitializeCapacityFromCurrentDeck(Owner);
        _pendingMoveFromDrawPile = true;

        bool moved = await TryMoveTopCardsIntoScorePile(new BlockingPlayerChoiceContext());
        if (moved) {
            _pendingMoveFromDrawPile = false;
        }
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState) {
        if (Owner == null || side != Owner.Creature.Side || !_pendingMoveFromDrawPile) {
            return;
        }

        await TryMoveTopCardsIntoScorePile(choiceContext);
        _pendingMoveFromDrawPile = false;
    }

    public override Task AfterCombatEnd(CombatRoom room) {
        _pendingMoveFromDrawPile = false;
        return Task.CompletedTask;
    }

    private async Task<bool> TryMoveTopCardsIntoScorePile(PlayerChoiceContext choiceContext) {
        if (Owner == null || !IsOwnerJera()) {
            return false;
        }

        CardPile drawPile = PileType.Draw.GetPile(Owner);
        if (drawPile == null) {
            return false;
        }

        List<CardModel> cards = new List<CardModel>();
        for (int i = 0; i < DynamicVars[AmountKey].IntValue; i++) {
            await CardPileCmd.ShuffleIfNecessary(choiceContext, Owner);
            CardModel topCard = drawPile.Cards.FirstOrDefault();
            if (topCard == null) {
                break;
            }

            topCard.RemoveFromCurrentPile();
            cards.Add(topCard);
        }

        if (cards.Count == 0) {
            return false;
        }

        Flash();
        await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, cards.ToArray());
        return true;
    }
}
