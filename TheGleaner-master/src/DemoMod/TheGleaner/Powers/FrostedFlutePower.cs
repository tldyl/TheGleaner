using DemoMod.TheGleaner.Cards.GleanerCard;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Powers;

public class FrostedFlutePower : TemporaryStrengthPower {
    public override AbstractModel OriginModel => ModelDb.Card<FrostedFlute>();
    
    protected override bool IsPositive => false;
}
