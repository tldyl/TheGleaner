using DemoMod.TheGleaner.Cards.GleanerCard;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace DemoMod.TheGleaner.Utils;


public class RandomDissonanceCard {
    public static List<CardModel> getRandomDissonanceCards(int count, Rng rng) {
        List<CardModel> pool = [
            ModelDb.Card<HowlOfWrath>(),
            ModelDb.Card<ShriekOfDread>(),
            ModelDb.Card<DirgeOfFarewell>()
        ];
        List<CardModel> ret = [];
        for (int i = 0; i < count; i++) {
            ret.Add(pool[rng.NextInt(pool.Count)]);
        }
        return ret;
    }

    public static List<CardModel> transformedDissonanceCards() {
        return [ModelDb.Card<ForgingAtDawn>(), ModelDb.Card<NightingaleAtTheAbyss>(), ModelDb.Card<PulsationOfTheTides>()];
    }
}
