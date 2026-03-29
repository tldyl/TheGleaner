using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace DemoMod.TheGleaner.Enums;

public class CustomEnums {
    [CustomEnum]
    public static PileType ScorePile;

    [CustomEnum]
    public static CardTag Arrow;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Resonance;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Concerto;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Dissonance;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Glean;
}
