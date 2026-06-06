using DemoMod.TheGleaner.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using System.Reflection;
using System.Reflection.Emit;

namespace TheGleaner.DemoMod.TheGleaner.Patches;

public class CreaturePatch {
    [HarmonyPatch]
    public static class PatchTakeTurn {
        static Type? GetNestedType() {
            Type parentClass = typeof(Creature);
            Type[] stateMachineTypes = parentClass.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public);
            foreach (Type type in stateMachineTypes) {
                if (type.Name.Contains("<TakeTurn>")) {
                    return type;
                }
            }
            return null;
        }
        
        static MethodBase TargetMethod() {
            Type? type = GetNestedType();
            if (type != null) {
                Log.Info("Find TakeTurn method.");
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
                if (instructionList[i].opcode == OpCodes.Call && instructionList[i].operand.Equals(AccessTools.PropertyGetter(typeof(Creature), "Monster"))
                    && instructionList[i + 1].opcode == OpCodes.Callvirt && instructionList[i + 1].operand.Equals(AccessTools.Method(typeof(MonsterModel), "PerformMove", []))) {
                    foundIndex = i + 4;
                    break;
                }
            }
            if (foundIndex == -1) {
                throw new Exception("Could not find target line.");
            }
            List<CodeInstruction> insertedCode = [
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Creature), "Monster")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchTakeTurn), nameof(Insert), [typeof(MonsterModel)]))
            ];
            instructionList.InsertRange(foundIndex, insertedCode);

            return instructionList;
        }

        public static void Insert(MonsterModel monster) {
            if (monster.Creature.HasPower<MakerAppearPower>()) {
                TaskHelper.RunSafely(monster.PerformMove());
            }
        }
    }
}
