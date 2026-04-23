using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace DemoMod.TheGleaner.Patches.ScoreEntryCardPatch;

[HarmonyPatch(typeof(NCard), "Reload")]
public static class ScoreEntryCardFramePatch
{
	private static Texture2D? _scoreFrame;

	private static Texture2D? TryLoadTexture(params string[] paths)
	{
		foreach (string path in paths)
		{
			try
			{
				Texture2D tex = ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Reuse);
				if (tex != null)
				{
					return tex;
				}
				Image image = new Image();
				Error err = image.Load(path);
				if (err == Error.Ok)
				{
					return ImageTexture.CreateFromImage(image);
				}
			}
			catch (Exception)
			{
			}
		}
		return null;
	}

	private static TextureRect? FindTextureRectByName(Node root, params string[] names)
	{
		for (int i = 0; i < names.Length; i++)
		{
			TextureRect byUnique = root.GetNodeOrNull<TextureRect>(names[i]);
			if (byUnique != null)
			{
				return byUnique;
			}
		}
		foreach (Node child in root.GetChildren())
		{
			if (child is TextureRect rect)
			{
				for (int j = 0; j < names.Length; j++)
				{
					string candidate = names[j].TrimStart('%');
					if (string.Equals(rect.Name.ToString(), candidate, StringComparison.OrdinalIgnoreCase))
					{
						return rect;
					}
				}
			}
			TextureRect nested = FindTextureRectByName(child, names);
			if (nested != null)
			{
				return nested;
			}
		}
		return null;
	}

	private static void ClearTextureRect(Node root, params string[] names)
	{
		TextureRect target = FindTextureRectByName(root, names);
		if (target != null)
		{
			target.Texture = null;
			target.Visible = false;
		}
	}

	private static int HideNodesByNameContains(Node root, params string[] fragments)
	{
		int hiddenCount = 0;
		string nodeName = root.Name.ToString();
		bool shouldHide = false;
		for (int i = 0; i < fragments.Length; i++)
		{
			if (nodeName.Contains(fragments[i], StringComparison.OrdinalIgnoreCase))
			{
				shouldHide = true;
				break;
			}
		}
		if (shouldHide && root is CanvasItem canvasItem)
		{
			canvasItem.Visible = false;
			if (!(root is TextureRect textureRect))
			{
				if (!(root is Sprite2D sprite2D))
				{
					if (root is NinePatchRect ninePatchRect)
					{
						ninePatchRect.Texture = null;
					}
				}
				else
				{
					sprite2D.Texture = null;
				}
			}
			else
			{
				textureRect.Texture = null;
			}
			hiddenCount++;
		}
		foreach (Node child in root.GetChildren())
		{
			hiddenCount += HideNodesByNameContains(child, fragments);
		}
		return hiddenCount;
	}

	private static void ApplyScoreEntryCardStyle(NCard cardNode)
	{
		TextureRect frameNode = FindTextureRectByName((Node)(object)cardNode, "%Frame", "Frame");
		if (frameNode != null) {
			frameNode.Visible = false;
		}
		ClearTextureRect((Node)(object)cardNode, "%EnergyIcon", "EnergyIcon");
		ClearTextureRect((Node)(object)cardNode, "%CardBanner", "%Banner", "CardBanner", "Banner");
		ClearTextureRect((Node)(object)cardNode, "%CardPortraitBorder", "%PortraitBorder", "%PortraitFrame", "CardPortraitBorder", "PortraitBorder", "PortraitFrame");
		HideNodesByNameContains((Node)(object)cardNode, "banner");
		HideNodesByNameContains((Node)(object)cardNode, "portrait_border_plaque", "portraitborderplaque", "border_plaque", "plaque");
		cardNode.GetNode<Node2D>("CardContainer/ScoreCardVfx").Visible = true;
	}

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
				if (!__instance.HasNode("CardContainer/ScoreCardVfx")) {
					Node2D scoreCardVfx = PreloadManager.Cache.GetScene("res://TheGleaner/scenes/score_card_vfx.tscn").Instantiate<Node2D>();
					scoreCardVfx.Position = new Vector2(-158.0f, -211.0f);
					__instance.GetNode("CardContainer").AddChild(scoreCardVfx);
					__instance.GetNode("CardContainer").MoveChild(scoreCardVfx, __instance.GetNode("%Frame").GetIndex() + 1);
				}
				if (string.Equals(cardId, "DEMOMOD-SCORE_ENTRY_CARD", StringComparison.OrdinalIgnoreCase)) {
					ApplyScoreEntryCardStyle(__instance);
				} else {
					__instance.GetNode<Node2D>("CardContainer/ScoreCardVfx").Visible = false;
				}
			}
		}
		catch (Exception)
		{
		}
	}
}
