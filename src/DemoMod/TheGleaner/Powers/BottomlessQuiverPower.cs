using BaseLib.Abstracts;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace DemoMod.TheGleaner.Powers;

public class BottomlessQuiverPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private int triggersUsedThisTurn = 0;

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        CombatState combatState)
    {
        if (side == Owner.Side)
        {
            triggersUsedThisTurn = 0;
        }
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (!Owner.IsPlayer)
        {
            return;
        }

        if (!cardPlay.Card.Tags.Contains(CardTag.Strike) &&
            !cardPlay.Card.Tags.Contains(CustomEnums.Arrow))
        {
            return;
        }

        if (triggersUsedThisTurn >= Amount)
        {
            return;
        }

        var player = Owner.Player;
        if (player == null)
        {
            return;
        }

        triggersUsedThisTurn++;

        await ScorePileCmd.AddCards(player.PlayerCombatState, player, cardPlay.Card);
        await PlayerCmd.GainEnergy(1, player);
    }
}