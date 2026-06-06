using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using DemoMod.TheGleaner.Cards.TokenCards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using DemoMod.TheGleaner.Monsters;
using MegaCrit.Sts2.Core.HoverTips;

namespace DemoMod.TheGleaner.Powers;

public class TalkativePower : CustomPowerModel {
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Talkative>()
    ];
    
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState) {
        if (side == Owner.Side) {
            await CardPileCmd.AddGeneratedCardsToCombat(
                [CombatState.CreateCard(ModelDb.Card<Talkative>(), Owner.Player)],
                PileType.Hand,
                true,
                CardPilePosition.Top
            );
        }
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength) {
        if (creature is {IsMonster: true, Monster: SirenCultist}) {
            await PowerCmd.Decrement(this);
        }
    }
}
