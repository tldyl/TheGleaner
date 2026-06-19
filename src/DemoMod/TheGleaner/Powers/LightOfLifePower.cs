using BaseLib.Abstracts;
using DemoMod.TheGleaner.Afflictions;
using DemoMod.TheGleaner.Cards.TokenCards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Powers;

public class LightOfLifePower : CustomPowerModel {
    public override string CustomPackedIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string CustomBigIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<ArbiterOfLife>()
    ];

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState) {
        if (side == Owner.Side) {
            foreach (CardModel card in Owner.Player.PlayerCombatState.AllCards) {
                if (card.Affliction is LightOfLife) {
                    card.Affliction.Amount = 2;
                }
            }
            await CardPileCmd.AddGeneratedCardsToCombat(
                [CombatState.CreateCard(ModelDb.Card<ArbiterOfLife>(), Owner.Player)],
                PileType.Hand,
                Owner.Player,
                CardPilePosition.Top
            );
        }
    }
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
        AfflictionModel affliction = cardPlay.Card.Affliction;
        if (affliction is LightOfLife) {
            Flash();
            foreach (Creature creature in Owner.CombatState.Enemies) {
                if (creature.HasPower<DeclarationOfTheEndPower>()) {
                    switch (affliction.Amount) {
                        case 3:
                            await PowerCmd.Apply<DemoTempStrengthPower>(context, Owner, 2, Owner, null);
                            break;
                        case 4:
                            await PowerCmd.Apply<HotfixPower>(context, Owner, 1, Owner, null);
                            break;
                        case 5:
                            await CreatureCmd.Heal(Owner, 4);
                            break;
                    }
                }
            }
        }
    }
}
