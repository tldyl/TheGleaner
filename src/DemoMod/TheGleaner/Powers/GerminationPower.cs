using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace DemoMod.TheGleaner.Powers;

public class GerminationPower : CustomPowerModel {
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<EtchPower>()];

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState) {
        if (side != Owner.Side)
            return;
        Flash();
        await Cmd.CustomScaledWait(0.2f, 0.4f);
        foreach (Creature hittableEnemy in CombatState.HittableEnemies) {
            NCreature creatureNode = NCombatRoom.Instance?.GetCreatureNode(hittableEnemy);
            if (creatureNode != null)
                NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(NGaseousImpactVfx.Create(creatureNode.VfxSpawnPosition, new Color("5c0026")));
        }
        await PowerCmd.Apply<EtchPower>(CombatState.HittableEnemies, Amount, Owner, null);
    }
}
