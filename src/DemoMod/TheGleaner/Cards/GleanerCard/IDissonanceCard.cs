using MegaCrit.Sts2.Core.Entities.Players;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

/// <summary>
/// 不和谐音请实现此接口
/// </summary>
public interface IDissonanceCard {
    public void OnEnterScorePile(PlayerCombatState combatState, Player player);
}
