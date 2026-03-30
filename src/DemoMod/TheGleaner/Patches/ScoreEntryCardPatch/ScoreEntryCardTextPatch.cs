using System;
using System.Collections.Generic;
using DemoMod.TheGleaner.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace DemoMod.TheGleaner.Patches.ScoreEntryCardPatch;

public static class ScoreEntryCardTextPatch {
    private static readonly object _bindingLock = new();
    private static readonly Dictionary<ulong, string> _lastReloadCardByInstanceId = new();
    private static readonly Dictionary<ulong, string> _lastVisualCardByInstanceId = new();
    private static readonly HashSet<ulong> _instancesWithForcedHiddenText = [];

    private static string GetNodePathForLog(Node node) {
        List<string> segments = [];
        Node? current = node;
        while (current != null) {
            segments.Add(current.Name.ToString());
            current = current.GetParent();
        }
        segments.Reverse();
        return "/" + string.Join("/", segments);
    }

    private static void LogCardBinding(string hookName, NCard cardNode, string cardId) {
        ulong instanceId = cardNode.GetInstanceId();
        string nodePath = GetNodePathForLog(cardNode);
        Dictionary<ulong, string> tracker = hookName == "Reload"
            ? _lastReloadCardByInstanceId
            : _lastVisualCardByInstanceId;

        lock (_bindingLock) {
            if (tracker.TryGetValue(instanceId, out string? previousCardId)) {
                if (!string.Equals(previousCardId, cardId, StringComparison.OrdinalIgnoreCase)) {
                }
            } else {}

            tracker[instanceId] = cardId;
        }
    }

    private static bool HideLabelText(NCard root, string cardId, string label, params string[] names) {

        foreach (string name in names) {
            MegaLabel? megaLabel = root.GetNodeOrNull<MegaLabel>(name);
            if (megaLabel != null) {
                megaLabel.SetTextAutoSize(string.Empty);
                megaLabel.Visible = false;
                return true;
            }
        }

        foreach (Node child in root.GetChildren()) {
            if (HideLabelTextRecursive(child, cardId, label, names)) {
                return true;
            }
        }

        return false;
    }

    private static bool HideLabelTextRecursive(Node node, string cardId, string label, params string[] names) {
        if (node is MegaLabel megaLabel) {
            string nodeName = megaLabel.Name.ToString();
            for (int i = 0; i < names.Length; i++) {
                string candidate = names[i].TrimStart('%');
                if (string.Equals(nodeName, candidate, StringComparison.OrdinalIgnoreCase)) {
                    megaLabel.SetTextAutoSize(string.Empty);
                    megaLabel.Visible = false;
                    return true;
                }
            }
        }

        foreach (Node child in node.GetChildren()) {
            if (HideLabelTextRecursive(child, cardId, label, names)) {
                return true;
            }
        }

        return false;
    }

    private static MegaLabel? FindMegaLabelByName(Node root, params string[] names) {
        for (int i = 0; i < names.Length; i++) {
            MegaLabel? byUnique = root.GetNodeOrNull<MegaLabel>(names[i]);
            if (byUnique != null) {
                return byUnique;
            }
        }

        foreach (Node child in root.GetChildren()) {
            if (child is MegaLabel megaLabel) {
                string nodeName = megaLabel.Name.ToString();
                for (int i = 0; i < names.Length; i++) {
                    string candidate = names[i].TrimStart('%');
                    if (string.Equals(nodeName, candidate, StringComparison.OrdinalIgnoreCase)) {
                        return megaLabel;
                    }
                }
            }

            MegaLabel? nested = FindMegaLabelByName(child, names);
            if (nested != null) {
                return nested;
            }
        }

        return null;
    }

    private static void RestoreLabelVisibilityIfNeeded(NCard cardNode, string cardId, string hookName) {
        ulong instanceId = cardNode.GetInstanceId();
        bool shouldRestore;
        lock (_bindingLock) {
            shouldRestore = _instancesWithForcedHiddenText.Contains(instanceId);
        }

        if (!shouldRestore) {
            return;
        }

        int restored = 0;
        MegaLabel? title = FindMegaLabelByName(cardNode, "%TitleLabel", "TitleLabel");
        if (title != null && !title.Visible) {
            title.Visible = true;
            restored++;
                  }

        MegaLabel? energy = FindMegaLabelByName(cardNode, "%EnergyLabel", "EnergyLabel");
        if (energy != null && !energy.Visible) {
            energy.Visible = true;
            restored++;
              }

        if (restored > 0) {
         } else {
         }

        lock (_bindingLock) {
            _instancesWithForcedHiddenText.Remove(instanceId);
        }
    }

    private static void Apply(NCard cardNode, string cardId) {
        ulong instanceId = cardNode.GetInstanceId();
        bool titleHidden = HideLabelText(cardNode, cardId, "title", "%TitleLabel", "TitleLabel");
        bool energyHidden = HideLabelText(cardNode, cardId, "energy cost text", "%EnergyLabel", "EnergyLabel");
        if (titleHidden || energyHidden) {
            lock (_bindingLock) {
                _instancesWithForcedHiddenText.Add(instanceId);
            }
        }
      }

    [HarmonyPatch(typeof(NCard), "Reload")]
    public static class NCardReloadPatch {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(NCard __instance) {
            try {
                if (__instance == null) {
                    return;
                }

                string cardId = __instance.Model?.Id?.Entry ?? string.Empty;
                LogCardBinding("Reload", __instance, cardId);
                if (!string.Equals(cardId, "DEMOMOD-SCORE_ENTRY_CARD", StringComparison.OrdinalIgnoreCase)) {
                    RestoreLabelVisibilityIfNeeded(__instance, cardId, "Reload");
                    return;
                }

                Apply(__instance, cardId);
            } catch (Exception e) {
                      }
        }
    }

    [HarmonyPatch(typeof(NCard), "UpdateVisuals")]
    public static class NCardUpdateVisualsPatch {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(NCard __instance) {
            try {
                if (__instance == null) {
                    return;
                }

                string cardId = __instance.Model?.Id?.Entry ?? string.Empty;
                LogCardBinding("UpdateVisuals", __instance, cardId);
                if (!string.Equals(cardId, "DEMOMOD-SCORE_ENTRY_CARD", StringComparison.OrdinalIgnoreCase)) {
                    RestoreLabelVisibilityIfNeeded(__instance, cardId, "UpdateVisuals");
                    return;
                }

                Apply(__instance, cardId);
                   } catch (Exception e) {
                         }
        }
    }
}
