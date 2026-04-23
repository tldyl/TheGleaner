using HarmonyLib;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using System.Runtime.CompilerServices;

namespace DemoMod.TheGleaner.ConsoleCommands;

public class DumpNetCombatCardDbConsoleCmd : AbstractConsoleCmd {
    public override string CmdName => "dump-net-combat-carddb";
    public override string Args => "";
    public override string Description => "";
    public override bool IsNetworked => false;
    
    public override CmdResult Process(Player? issuingPlayer, string[] args) {
        Dictionary<uint, CardModel> _idToCard = (Dictionary<uint, CardModel>) AccessTools.Field(typeof(NetCombatCardDb), "_idToCard").GetValue(NetCombatCardDb.Instance);
        DefaultInterpolatedStringHandler stringBuilder = new DefaultInterpolatedStringHandler(16, 3);
        foreach ((uint id, CardModel card) in _idToCard) {
            stringBuilder.AppendLiteral(id.ToString());
            stringBuilder.AppendLiteral(" ");
            stringBuilder.AppendLiteral(card.Owner.NetId.ToString());
            stringBuilder.AppendLiteral(": ");
            stringBuilder.AppendLiteral(card.Title);
            stringBuilder.AppendLiteral("\n");
        }
        return new CmdResult(true, stringBuilder.ToStringAndClear());
    }
}
