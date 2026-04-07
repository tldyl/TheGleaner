using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Glissando : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar("Amount", 1),
        new DamageVar(5, ValueProp.Move),
        new PowerVar<VulnerablePower>(1)   // ✅ 新增 Vulnerable
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()  // ✅ 新增提示
    ];

    public Glissando() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AllEnemies) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        Vector2 windowSize = NRun.Instance.CombatRoom.Ui.GetViewport().GetVisibleRect().Size;
        GleanerVfxCmd.PlayVfx(new Vector2(windowSize.X * 0.65f, windowSize.Y * 0.67f), "res://TheGleaner/scenes/vfx/aoe_attack_demo.tscn", 0.5f);
        await CreatureCmd.TriggerAnim(Owner.Creature, "AoEAttack", 0.5f);

        IEnumerable<DamageResult> damageResults =
            await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, DynamicVars.Damage, Owner.Creature, this);


        // 原逻辑：几乎全杀 → 上 Weak
        int count = damageResults.Count(result => result.WasTargetKilled);

        if (count == damageResults.Count() - 1) {
            await PowerCmd.Apply<WeakPower>(
                CombatState.HittableEnemies,
                DynamicVars["Amount"].BaseValue,
                Owner.Creature,
                this
            );
                    // ✅ 先上 Vulnerable（类似 Thunderclap）
            await PowerCmd.Apply<VulnerablePower>(
                CombatState.HittableEnemies,
                DynamicVars.Vulnerable.BaseValue,
                Owner.Creature,
                this
             );  
        }
        

    }

    protected override void OnUpgrade() {
        DynamicVars.Damage.UpgradeValueBy(3);
        // （可选）如果你想升级也加 Vulnerable，可以再加一行：
        // DynamicVars.Vulnerable.UpgradeValueBy(1);
    }
}