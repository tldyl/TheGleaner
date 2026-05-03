using DemoMod.TheGleaner.Cards.GleanerCard;
using DemoMod.TheGleaner.Commands;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
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

    [HarmonyPatch(typeof(NPlayerHand), "SelectCardInSimpleMode")]
    public static class PatchSelectCardInSimpleMode {
        public static void Postfix(NPlayerHand __instance, NHandCardHolder? holder) {
            if (ScorePileCmd.gleanCard) {
                List<CardModel> _selectedCards = AccessTools.Field(typeof(NPlayerHand), "_selectedCards").GetValue(__instance) as List<CardModel>;
                CardSelectorPrefs prefs = (CardSelectorPrefs) AccessTools.Field(typeof(NPlayerHand), "_prefs").GetValue(__instance);
                MegaRichTextLabel _selectionHeader = AccessTools.Field(typeof(NPlayerHand), "_selectionHeader").GetValue(__instance) as MegaRichTextLabel;
                if (_selectedCards.Count == 0) {
                    AccessTools.Field(typeof(LocString), "<locEntryKey>P").SetValue(prefs.Prompt, "DEMOMOD-WINDS_MUSE.selectionScreenPromptDrawPileOnly");
                } else if (_selectedCards.Count == prefs.MaxSelect) {
                    AccessTools.Field(typeof(LocString), "<locEntryKey>P").SetValue(prefs.Prompt, "DEMOMOD-WINDS_MUSE.selectionScreenPromptHandOnly");
                } else {
                    AccessTools.Field(typeof(LocString), "<locEntryKey>P").SetValue(prefs.Prompt, "DEMOMOD-WINDS_MUSE.selectionScreenPromptDrawPileAndHand");
                }
                ((IntVar) prefs.Prompt.Variables["HandAmount"]).BaseValue = _selectedCards.Count;
                ((IntVar) prefs.Prompt.Variables["DrawAmount"]).BaseValue = prefs.MaxSelect - _selectedCards.Count;
                _selectionHeader.Text = "[center]" + prefs.Prompt.GetFormattedText() + "[/center]";
            }
        }
    }

    [HarmonyPatch(typeof(NPlayerHand), "DeselectCard")]
    public static class PatchDeselectCard {
        public static void Postfix(NPlayerHand __instance, NCard card) {
            PatchSelectCardInSimpleMode.Postfix(__instance, null);
        }
    }
}
