using DemoMod.TheGleaner.Cards.GleanerCard;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Powers;

public class SwiftArrowPower : TemporaryStrengthPower {
    public override AbstractModel OriginModel => ModelDb.Card<SwiftArrow>();
    
    protected override bool IsPositive => false;
}
