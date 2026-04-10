using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace DemoMod.TheGleaner.UI;

public partial class NGleanerMerchantCharacter : NMerchantCharacter {
    public override void _Ready() {
        PlayAnimation("idle_loop", true);
    }
}
