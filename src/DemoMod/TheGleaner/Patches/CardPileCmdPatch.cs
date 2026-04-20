using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using System.Reflection;
using System.Reflection.Emit;

namespace DemoMod.TheGleaner.Patches;
public class CardPileCmdPatch {
    [HarmonyPatch]
    public static class PatchAdd {
        static Type? GetNestedType() {
            Type parentClass = typeof(CardPileCmd);
            Type[] stateMachineTypes = parentClass.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public);
            foreach (Type type in stateMachineTypes) {
                if (type.Name.Contains(nameof(CardPileCmd.Add)) && type.Name.Contains("d__9")) {
                    return type;
                }
            }
            return null;
        }
        
        static MethodBase TargetMethod() {
            Type? type = GetNestedType();
            if (type != null) {
                Log.Info("Find add method.");
                MethodInfo moveNext = type.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (moveNext != null) {
                    return moveNext;
                }
            }
            return null;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            List<CodeInstruction> instructionList = instructions.ToList();
            int foundIndex = -1;
            for (int i = 0; i < instructionList.Count; i++) {
                CodeInstruction instruction = instructionList[i];
                if (instruction.opcode == OpCodes.Ldstr && instruction.operand.Equals("HAND_FULL")) {
                    foundIndex = i;
                    break;
                }
            }
            if (foundIndex != -1) {
                Label elseLabel = generator.DefineLabel();
                Label endLabel = generator.DefineLabel();
                
                instructionList.RemoveAt(foundIndex);
                
                List<CodeInstruction> insertCodes = [
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(GetNestedType(), "<owningPlayer>5__3")),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), "Character")),
                    new CodeInstruction(OpCodes.Isinst, typeof(Characters.TheGleaner)),
                    new CodeInstruction(OpCodes.Ldnull),
                    new CodeInstruction(OpCodes.Cgt_Un),
                    new CodeInstruction(OpCodes.Brfalse_S, elseLabel),
                    new CodeInstruction(OpCodes.Ldstr, "JERA_HAND_FULL"),
                    new CodeInstruction(OpCodes.Br_S, endLabel),
                    new CodeInstruction(OpCodes.Ldstr, "HAND_FULL") {labels = [elseLabel]},
                    new CodeInstruction(OpCodes.Nop) {labels = [endLabel]}
                ];
                
                instructionList.InsertRange(foundIndex, insertCodes);
            }
            return instructionList;
        }
    }
    
    [HarmonyPatch(typeof(CardPileCmd), "CheckIfDrawIsPossibleAndShowThoughtBubbleIfNot")]
    public static class PatchCheckIfDrawIsPossibleAndShowThoughtBubbleIfNot {
        public static bool Prefix(Player player, ref bool __result) {
            if (player.Character is Characters.TheGleaner) {
                if (PileType.Draw.GetPile(player).Cards.Count + PileType.Discard.GetPile(player).Cards.Count == 0) {
                    ThinkCmd.Play(new LocString("combat_messages", "JERA_NO_DRAW"), player.Creature, 2.0);
                    __result = false;
                    return false;
                }
                if (PileType.Hand.GetPile(player).Cards.Count < 10) {
                    __result = true;
                    return false;
                }
                __result = false;
                ThinkCmd.Play(new LocString("combat_messages", "JERA_HAND_FULL"), player.Creature, 2.0);
            }
            return player.Character is not Characters.TheGleaner;
        }
    }
}
