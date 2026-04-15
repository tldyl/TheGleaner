using DemoMod.TheGleaner.Cards.GleanerCard;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using System.Reflection;
using System.Reflection.Emit;

namespace DemoMod.TheGleaner.Patches.ScoreEntryCardPatch;

public class NPlayerHandPatch {
    [HarmonyPatch(typeof(NPlayerHand), "UpdateSelectModeCardVisibility")]
    public static class PatchUpdateSelectModeCardVisibility {
        public static void Postfix(NPlayerHand __instance) {
            NHandCardHolder holder = __instance.ActiveHolders.Where(holder => holder.CardModel is ScoreEntryCard).FirstOrDefault();
            if (holder != null && __instance.CurrentMode is NPlayerHand.Mode.SimpleSelect or NPlayerHand.Mode.UpgradeSelect) {
                holder.Visible = false;
                holder.UpdateCard();
                __instance.ForceRefreshCardIndices();
            }
        }
    }

    [HarmonyPatch(typeof(NPlayerHand), "StartCardPlay")]
    public static class PatchStartCardPlay {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase originalMethod) {
            List<CodeInstruction> codeList = [..instructions];
            FieldInfo draggedHolderIndexField = AccessTools.Field(
                originalMethod.DeclaringType,
                "_draggedHolderIndex"
            );
            int foundIndex = -1;
            for (int i = 0; i < codeList.Count; i++) {
                if (codeList[i].opcode == OpCodes.Stfld && (codeList[i].operand as FieldInfo)?.Name == "_draggedHolderIndex") {
                    foundIndex = i + 1;
                    break;
                }
            }
            if (foundIndex != -1) {
                Label skipAssignLabel = generator.DefineLabel();
                List<CodeInstruction> insertInstructions = [];
                insertInstructions.AddRange(
                    // 1. 加载 this
                    new CodeInstruction(OpCodes.Ldarg_0),
                    // 2. 加载字段 _draggedHolderIndex
                    new CodeInstruction(OpCodes.Ldfld, draggedHolderIndexField),
                    // 3. 加载常量 10
                    new CodeInstruction(OpCodes.Ldc_I4_S, 10),
                    // 4. 比较：< 10 则跳过
                    new CodeInstruction(OpCodes.Blt_S, skipAssignLabel),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldc_I4_S, 9),
                    new CodeInstruction(OpCodes.Stfld, draggedHolderIndexField),
                    new CodeInstruction(OpCodes.Nop) {labels = [skipAssignLabel]}
                );
                codeList.InsertRange(foundIndex, insertInstructions);
            }
            return codeList;
        }
    }
}
