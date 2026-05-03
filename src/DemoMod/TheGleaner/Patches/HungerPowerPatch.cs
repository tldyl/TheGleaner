using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Reflection.Emit;

namespace TheGleaner.DemoMod.TheGleaner.Patches;

public class HungerPowerPatch {
    [HarmonyPatch(typeof(HungerPower), "AfterRemoved")]
    public static class PatchAfterRemoved {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            List<CodeInstruction> instrList = instructions.ToList();
            Label loopEndLabel = generator.DefineLabel();
            instrList[56].labels.Add(loopEndLabel);

            // ==============================================
            // 第二步：插入我们的null判断代码
            // ==============================================
            // 获取当前位置的card加载指令，自动提取card的局部变量索引
            // 兼容所有ldloc形式（ldloc.0/ldloc.s/ldloc）

            // 插入的IL代码，等价于C#：if (card.Affliction == null) continue;
            List<CodeInstruction> newInstructions = [
                // 加载card变量
                new CodeInstruction(OpCodes.Ldloc_S, 4),
                // 获取Affliction
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(CardModel), nameof(CardModel.Affliction))),
                // 加载null值
                new CodeInstruction(OpCodes.Ldnull),
                // 比较：card.Affliction == null，结果为bool值
                new CodeInstruction(OpCodes.Ceq),
                // 如果结果为true（card为null），跳转到循环结束，实现continue
                new CodeInstruction(OpCodes.Brtrue_S, loopEndLabel)
            ];

            instrList.InsertRange(41, newInstructions);

            // 返回修改后的IL指令列表
            return instrList;
        }
    }
}
