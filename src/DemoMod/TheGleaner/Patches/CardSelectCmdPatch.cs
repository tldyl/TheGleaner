using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using System.Reflection;
using System.Reflection.Emit;

namespace TheGleaner.DemoMod.TheGleaner.Patches;

public class CardSelectCmdPatch {
    [HarmonyPatch]
    public static class PatchFromChooseACardScreen {
        static MethodBase TargetMethod() {
            Type parentClass = typeof(CardSelectCmd);
            Type[] stateMachineTypes = parentClass.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public);
            foreach (Type type in stateMachineTypes) {
                if (type.Name.Contains(nameof(CardSelectCmd.FromChooseACardScreen))) {
                    MethodInfo moveNext = type.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (moveNext != null) {
                        return moveNext;
                    }
                }
            }
            return null;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            List<CodeInstruction> codeList = new List<CodeInstruction>(instructions);
            int foundIndex = -1;

            for (int i = 0; i < codeList.Count; i++) {
                if (codeList[i].opcode == OpCodes.Throw) {
                    foundIndex = i;
                    break;
                }
            }

            if (foundIndex != -1) {
                codeList[foundIndex] = new CodeInstruction(OpCodes.Nop);
                codeList[foundIndex - 1] = new CodeInstruction(OpCodes.Nop);
                codeList[foundIndex - 2] = new CodeInstruction(OpCodes.Nop);
                codeList[foundIndex - 3] = new CodeInstruction(OpCodes.Nop);
            }
            
            return codeList;
        }
    }
}
