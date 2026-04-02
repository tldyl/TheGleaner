using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class SuperpositionPrep : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    private bool _pendingNextTurn;
    private bool _armed;

    public SuperpositionPrep() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        _pendingNextTurn = true;
        _armed = false;
        await Task.CompletedTask;
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState) {
        if (_pendingNextTurn && side == Owner.Creature.Side) {
            _pendingNextTurn = false;
            _armed = true;
        }
        await Task.CompletedTask;
    }

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount) {
        if (!_armed || card.Owner != Owner) {
            return playCount;
        }

        return playCount + 1;
    }

    public override async Task AfterModifyingCardPlayCount(CardModel card) {
        if (_armed && card.Owner == Owner) {
            _armed = false;
        }
        await Task.CompletedTask;
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
