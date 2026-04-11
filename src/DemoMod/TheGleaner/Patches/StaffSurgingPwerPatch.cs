using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using DemoMod.TheGleaner.Powers;

namespace DemoMod.TheGleaner.Patches;

/// <summary>
/// 让自定义 StaffSurgingPower 像原版 DebilitatePower 一样，接入 Weak / Vulnerable 的倍率结算链。
/// </summary>
[HarmonyPatch]
public static class StaffSurgingPowerPatch
{
    /// <summary>
    /// WeakPower 的原版逻辑会在这里查 dealer 身上的 DebilitatePower，
    /// 我们在 Postfix 里补查 StaffSurgingPower。
    /// </summary>
    [HarmonyPatch(typeof(WeakPower), nameof(WeakPower.ModifyDamageMultiplicative))]
    [HarmonyPostfix]
    public static void WeakPower_ModifyDamageMultiplicative_Postfix(
        WeakPower __instance,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature dealer,
        CardModel cardSource,
        ref decimal __result)
    {
        if (dealer == null)
        {
            return;
        }

        var staffSurging = dealer.GetPower<StaffSurgingPower>();
        if (staffSurging == null)
        {
            return;
        }

        // __result 此时已经是 WeakPower 原版算出来的倍率（已包含 Paper Krane / Debilitate 等）。
        // StaffSurgingPower 的逻辑是“本回合虚弱效果翻倍”，因此继续在这个倍率上做一次变换。
        __result = staffSurging.ModifyWeakMultiplier(target, __result, props, dealer, cardSource);
    }

    /// <summary>
    /// VulnerablePower 的原版逻辑会在这里查 target 身上的 DebilitatePower，
    /// 我们在 Postfix 里补查 StaffSurgingPower。
    /// </summary>
    [HarmonyPatch(typeof(VulnerablePower), nameof(VulnerablePower.ModifyDamageMultiplicative))]
    [HarmonyPostfix]
    public static void VulnerablePower_ModifyDamageMultiplicative_Postfix(
        VulnerablePower __instance,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature dealer,
        CardModel cardSource,
        ref decimal __result)
    {
        if (target == null)
        {
            return;
        }

        var staffSurging = target.GetPower<StaffSurgingPower>();
        if (staffSurging == null)
        {
            return;
        }

        // __result 此时已经是 VulnerablePower 原版算出来的倍率（已包含 Paper Phrog / Debilitate 等）。
        // StaffSurgingPower 的逻辑是“本回合易伤效果翻倍”，因此继续在这个倍率上做一次变换。
        __result = staffSurging.ModifyVulnerableMultiplier(target, __result, props, dealer, cardSource);
    }
}