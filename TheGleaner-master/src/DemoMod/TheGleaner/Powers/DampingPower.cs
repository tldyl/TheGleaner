using DemoMod.TheGleaner.Cards.GleanerCard;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Powers;

public class DampingPower : TemporaryStrengthPower {
    public override AbstractModel OriginModel => ModelDb.Card<Damping>();

    protected override bool IsPositive => false;
}
