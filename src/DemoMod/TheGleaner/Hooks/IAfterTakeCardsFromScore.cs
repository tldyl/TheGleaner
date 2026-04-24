using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Hooks;

public interface IAfterTakeCardsFromScore {
    public Task AfterTakeCardsFromScore(CardModel card);
}
