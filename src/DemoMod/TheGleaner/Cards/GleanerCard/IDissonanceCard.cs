using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

/// <summary>
/// 不和谐音请实现此接口
/// </summary>
public interface IDissonanceCard {
    public void OnEnterScorePile(PlayerCombatState combatState, Player player);
    public Action<CardModel> TransformFollowupAction { get; set; }
}
