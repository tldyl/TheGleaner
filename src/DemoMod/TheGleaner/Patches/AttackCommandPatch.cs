using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Logging;
using System.Reflection;
using System.Reflection.Emit;

namespace TheGleaner.DemoMod.TheGleaner.Patches;
public class AttackCommandPatch {
    public static readonly SpireField<AttackCommand, List<string>> HitSfxGroup = new SpireField<AttackCommand, List<string>>(() => []);
    
    [HarmonyPatch]
    public static class PatchExecute {
        static Type? GetNestedType() {
            Type parentClass = typeof(AttackCommand);
            Type[] stateMachineTypes = parentClass.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public);
            foreach (Type type in stateMachineTypes) {
                if (type.Name.Contains(nameof(AttackCommand.Execute)) && type.Name.Contains("d__86")) {
                    return type;
                }
            }
            return null;
        }
        
        static MethodBase TargetMethod() {
            Type? type = GetNestedType();
            if (type != null) {
                Log.Info("Find execute method.");
                MethodInfo moveNext = type.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (moveNext != null) {
                    return moveNext;
                }
            }
            return null;
        }
        
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            List<CodeInstruction> instructionList = instructions.ToList();
            int foundIndex = -1;
            for (int i = 0; i < instructionList.Count; i++) {
                if (i < 2) {
                    continue;
                }
                CodeInstruction instruction = instructionList[i];
                if (instruction.opcode == OpCodes.Brfalse_S
                    && instructionList[i - 1].opcode == OpCodes.Call && AccessTools.PropertyGetter(typeof(AttackCommand), "HitSfx").Equals(instructionList[i - 1].operand)
                    && instructionList[i - 2].opcode == OpCodes.Ldloc_1) {
                    foundIndex = i;
                    Log.Info("Find target line. index=" + i);
                    break;
                }
            }
            if (foundIndex != -1) {
                List<CodeInstruction> insertCodes = [
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchExecute), nameof(Insert), [typeof(AttackCommand)]))
                ];
                
                instructionList.InsertRange(foundIndex + 1, insertCodes);
            }
            
            return instructionList;
        }

        public static void Insert(AttackCommand command) {
            List<string> sfxGroup = HitSfxGroup.Get(command);
            if (sfxGroup.Count > 0) {
                AccessTools.PropertySetter(typeof(AttackCommand), "HitSfx").Invoke(command, [sfxGroup[0]]);
                sfxGroup.RemoveAt(0);
            }
        }
    }
}
