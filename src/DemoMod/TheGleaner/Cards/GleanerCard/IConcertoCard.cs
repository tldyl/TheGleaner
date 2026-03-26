using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

/// <summary>
/// 协奏牌请实现此接口
/// </summary>
public interface IConcertoCard {
    public Task OnConcerto(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay);
}
