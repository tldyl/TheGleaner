using BaseLib.Abstracts;
using Godot;

namespace DemoMod.TheGleaner.Pools;

public class CardPool : CustomCardPoolModel {
    public override string Title => "Gleaner Card Pool";
    public override string EnergyColorName => "ironclad";
    public override Color DeckEntryCardColor => new Color(0.5f, 0.5f, 1f);
    public override bool IsColorless => false;
}
