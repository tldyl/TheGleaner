using BaseLib.Abstracts;
using DemoMod.TheGleaner.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace DemoMod.TheGleaner.Powers;

public class MatinsNotationPower : CustomPowerModel {
    public override string CustomPackedIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string CustomBigIconPath => $"res://TheGleaner/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player) {
        if (player != Owner.Player) {
            return;
        }
        await CardPileCmd.Draw(choiceContext, Amount, player);
        for (int _ = 0; _ < Amount; _++) {
            await CardPileCmd.ShuffleIfNecessary(choiceContext, player);
            if (PileType.Draw.GetPile(player).Cards.Count > 0) {
                await ScorePileCmd.AddCards(player.PlayerCombatState, player, PileType.Draw.GetPile(player).Cards[0]);
            }
        }
    }
}
