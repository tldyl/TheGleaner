using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;

namespace DemoMod.TheGleaner.ConsoleCommands;

public class PlaySfxConsoleCmd : AbstractConsoleCmd {
    public override string CmdName => "sfx";
    public override string Args => "<path:string>";
    public override string Description => "play sfx on specified path";
    public override bool IsNetworked => false;
    
    public override CmdResult Process(Player? issuingPlayer, string[]? args) {
        if (args == null || args.Length == 0) {
            return new CmdResult(false, "Sfx path not specified.");
        }
        SfxCmd.Play(args[0]);
        return new CmdResult(true);
    }
}
