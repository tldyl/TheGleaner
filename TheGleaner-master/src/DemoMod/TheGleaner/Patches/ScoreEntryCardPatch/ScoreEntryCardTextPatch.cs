using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace DemoMod.TheGleaner.Patches.ScoreEntryCardPatch;

public static class ScoreEntryCardTextPatch
{
	[HarmonyPatch(typeof(NCard), "Reload")]
	public static class NCardReloadPatch
	{
		[HarmonyPostfix]
		[HarmonyPriority(0)]
		public static void Postfix(NCard __instance)
		{
			try
			{
				if (__instance != null)
				{
					CardModel model = __instance.Model;
					object obj;
					if (model == null)
					{
						obj = null;
					}
					else
					{
						ModelId id = ((AbstractModel)model).Id;
						obj = ((id != null) ? id.Entry : null);
					}
					if (obj == null)
					{
						obj = string.Empty;
					}
					string cardId = (string)obj;
					LogCardBinding("Reload", __instance, cardId);
					if (!string.Equals(cardId, "DEMOMOD-SCORE_ENTRY_CARD", StringComparison.OrdinalIgnoreCase))
					{
						RestoreLabelVisibilityIfNeeded(__instance, cardId, "Reload");
					}
					else
					{
						Apply(__instance, cardId);
					}
				}
			}
			catch (Exception)
			{
			}
		}
	}

	[HarmonyPatch(typeof(NCard), "UpdateVisuals")]
	public static class NCardUpdateVisualsPatch
	{
		[HarmonyPostfix]
		[HarmonyPriority(0)]
		public static void Postfix(NCard __instance)
		{
			try
			{
				if (__instance != null)
				{
					CardModel model = __instance.Model;
					object obj;
					if (model == null)
					{
						obj = null;
					}
					else
					{
						ModelId id = ((AbstractModel)model).Id;
						obj = ((id != null) ? id.Entry : null);
					}
					if (obj == null)
					{
						obj = string.Empty;
					}
					string cardId = (string)obj;
					LogCardBinding("UpdateVisuals", __instance, cardId);
					if (!string.Equals(cardId, "DEMOMOD-SCORE_ENTRY_CARD", StringComparison.OrdinalIgnoreCase))
					{
						RestoreLabelVisibilityIfNeeded(__instance, cardId, "UpdateVisuals");
					}
					else
					{
						Apply(__instance, cardId);
					}
				}
			}
			catch (Exception)
			{
			}
		}
	}

	private static readonly object _bindingLock = new object();

	private static readonly Dictionary<ulong, string> _lastReloadCardByInstanceId = new Dictionary<ulong, string>();

	private static readonly Dictionary<ulong, string> _lastVisualCardByInstanceId = new Dictionary<ulong, string>();

	private static readonly HashSet<ulong> _instancesWithForcedHiddenText = new HashSet<ulong>();

	private static string GetNodePathForLog(Node node)
	{
		List<string> segments = new List<string>();
		for (Node current = node; current != null; current = current.GetParent())
		{
			segments.Add(current.Name.ToString());
		}
		segments.Reverse();
		return "/" + string.Join("/", segments);
	}

	private static void LogCardBinding(string hookName, NCard cardNode, string cardId)
	{
		ulong instanceId = ((GodotObject)(object)cardNode).GetInstanceId();
		string nodePath = GetNodePathForLog((Node)(object)cardNode);
		Dictionary<ulong, string> tracker = ((hookName == "Reload") ? _lastReloadCardByInstanceId : _lastVisualCardByInstanceId);
		lock (_bindingLock)
		{
			if (!tracker.TryGetValue(instanceId, out var previousCardId) || !string.Equals(previousCardId, cardId, StringComparison.OrdinalIgnoreCase))
			{
			}
			tracker[instanceId] = cardId;
		}
	}

	private static bool HideLabelText(NCard root, string cardId, string label, params string[] names)
	{
		foreach (string name in names)
		{
			MegaLabel megaLabel = ((Node)(object)root).GetNodeOrNull<MegaLabel>((NodePath)name);
			if (megaLabel != null)
			{
				megaLabel.SetTextAutoSize(string.Empty);
				((CanvasItem)(object)megaLabel).Visible = false;
				return true;
			}
		}
		foreach (Node child in ((Node)(object)root).GetChildren(includeInternal: false))
		{
			if (HideLabelTextRecursive(child, cardId, label, names))
			{
				return true;
			}
		}
		return false;
	}

	private static bool HideLabelTextRecursive(Node node, string cardId, string label, params string[] names)
	{
		MegaLabel megaLabel = (MegaLabel)(object)((node is MegaLabel) ? node : null);
		if (megaLabel != null)
		{
			string nodeName = ((Node)(object)megaLabel).Name.ToString();
			for (int i = 0; i < names.Length; i++)
			{
				string candidate = names[i].TrimStart('%');
				if (string.Equals(nodeName, candidate, StringComparison.OrdinalIgnoreCase))
				{
					megaLabel.SetTextAutoSize(string.Empty);
					((CanvasItem)(object)megaLabel).Visible = false;
					return true;
				}
			}
		}
		foreach (Node child in node.GetChildren())
		{
			if (HideLabelTextRecursive(child, cardId, label, names))
			{
				return true;
			}
		}
		return false;
	}

	private static MegaLabel? FindMegaLabelByName(Node root, params string[] names)
	{
		for (int i = 0; i < names.Length; i++)
		{
			MegaLabel byUnique = root.GetNodeOrNull<MegaLabel>(names[i]);
			if (byUnique != null)
			{
				return byUnique;
			}
		}
		foreach (Node child in root.GetChildren())
		{
			MegaLabel megaLabel = (MegaLabel)(object)((child is MegaLabel) ? child : null);
			if (megaLabel != null)
			{
				string nodeName = ((Node)(object)megaLabel).Name.ToString();
				for (int j = 0; j < names.Length; j++)
				{
					string candidate = names[j].TrimStart('%');
					if (string.Equals(nodeName, candidate, StringComparison.OrdinalIgnoreCase))
					{
						return megaLabel;
					}
				}
			}
			MegaLabel nested = FindMegaLabelByName(child, names);
			if (nested != null)
			{
				return nested;
			}
		}
		return null;
	}

	private static void RestoreLabelVisibilityIfNeeded(NCard cardNode, string cardId, string hookName)
	{
		ulong instanceId = ((GodotObject)(object)cardNode).GetInstanceId();
		bool shouldRestore;
		lock (_bindingLock)
		{
			shouldRestore = _instancesWithForcedHiddenText.Contains(instanceId);
		}
		if (!shouldRestore)
		{
			return;
		}
		int restored = 0;
		MegaLabel title = FindMegaLabelByName((Node)(object)cardNode, "%TitleLabel", "TitleLabel");
		if (title != null && !((CanvasItem)(object)title).Visible)
		{
			((CanvasItem)(object)title).Visible = true;
			restored++;
		}
		MegaLabel energy = FindMegaLabelByName((Node)(object)cardNode, "%EnergyLabel", "EnergyLabel");
		if (energy != null && !((CanvasItem)(object)energy).Visible)
		{
			((CanvasItem)(object)energy).Visible = true;
			restored++;
		}
		if (restored > 0)
		{
		}
		lock (_bindingLock)
		{
			_instancesWithForcedHiddenText.Remove(instanceId);
		}
	}

	private static void Apply(NCard cardNode, string cardId)
	{
		ulong instanceId = ((GodotObject)(object)cardNode).GetInstanceId();
		bool titleHidden = HideLabelText(cardNode, cardId, "title", "%TitleLabel", "TitleLabel");
		bool energyHidden = HideLabelText(cardNode, cardId, "energy cost text", "%EnergyLabel", "EnergyLabel");
		if (titleHidden || energyHidden)
		{
			lock (_bindingLock)
			{
				_instancesWithForcedHiddenText.Add(instanceId);
			}
		}
	}
}
