using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Powers;

public class WovenEmbracePower : CustomPowerModel {
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<PoisonPower>()
    ];

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult _,
        ValueProp props,
        Creature? dealer,
        CardModel? __) {
        if (target != Owner || dealer == null || !props.IsPoweredAttack())
            return;
        await PowerCmd.Apply<PoisonPower>(dealer, Amount, Owner, null);
    }
    
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side) {
        if (Owner.Side == side)
            return;
        await PowerCmd.Remove(this);
    }
}
