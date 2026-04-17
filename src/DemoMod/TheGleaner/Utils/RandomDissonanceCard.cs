using DemoMod.TheGleaner.Cards.GleanerCard;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace DemoMod.TheGleaner.Utils;
public class RandomDissonanceCard {
    private static List<CardModel> pool =
    [
        ModelDb.Card<HowlOfWrath>(),
        ModelDb.Card<ShriekOfDread>(),
        ModelDb.Card<DirgeOfFarewell>()
    ];

    public static void initPool() {
        pool = [
            ModelDb.Card<HowlOfWrath>(),
            ModelDb.Card<ShriekOfDread>(),
            ModelDb.Card<DirgeOfFarewell>()
        ];
    }
    
    public static List<CardModel> getRandomDissonanceCards(int count, Rng rng) {
        List<CardModel> ret = [];
        for (int i = 0; i < count; i++) {
            int roll = rng.NextInt(pool.Count);
            ret.Add(pool[roll]);
            pool.RemoveAt(roll);
            if (pool.Count == 0) {
                pool =
                [
                    ModelDb.Card<HowlOfWrath>(),
                    ModelDb.Card<ShriekOfDread>(),
                    ModelDb.Card<DirgeOfFarewell>()
                ];
            }
        }
        return ret;
    }

    public static List<CardModel> transformedDissonanceCards() {
        return [ModelDb.Card<ForgingAtDawn>(), ModelDb.Card<NightingaleAtTheAbyss>(), ModelDb.Card<PulsationOfTheTides>()];
    }
}
