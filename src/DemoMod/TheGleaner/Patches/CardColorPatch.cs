using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace DemoMod.TheGleaner.Patches;

[HarmonyPatch(typeof(NCard), "Reload")]
public static class CardColorPatch {
    private static readonly string[] _forceColorlessStatusFrameCardIds =
    [
        "DEMOMOD-DIRGE_OF_FAREWELL",
        "DEMOMOD-HOWL_OF_WRATH",
        "DEMOMOD-SHRIEK_OF_DREAD",
        "DEMOMOD-DIRGEOFFAREWELL",
        "DEMOMOD-HOWLOFWRATH",
        "DEMOMOD-SHRIEKOFDREAD"
    ];

    // 这几张牌强制恢复成游戏原版 colorless 外观
    private static readonly string[] _forceVanillaColorlessCardIds =
    [
        "DEMOMOD-ZERO_COST_ATTACKS",
        "DEMOMOD-ONE_COST_ATTACKS",
        "DEMOMOD-TWO_COST_ATTACKS",
        "DEMOMOD-THREE_OR_MORE_COST_ATTACKS",

        // 保险：兼容去下划线写法
        "DEMOMOD-ZEROCOSTATTACKS",
        "DEMOMOD-ONECOSTATTACKS",
        "DEMOMOD-TWOCOSTATTACKS",
        "DEMOMOD-THREEORMORECOSTATTACKS"
    ];

    // 不受这个 patch 影响的卡
    private static readonly string[] _excludedCardIds =
    [
        "DEMOMOD-SHRIEK_OF_DREAD",
        "DEMOMOD-HOWL_OF_WRATH",
        "DEMOMOD-DIRGE_OF_FAREWELL",
        "DEMOMOD-SHRIEKOFDREAD",
        "DEMOMOD-HOWLOFWRATH",
        "DEMOMOD-DIRGEOFFAREWELL",

        "DEMOMOD-NIGHTINGALE_AT_THE_ABYSS",
        "DEMOMOD-FORGING_AT_DAWN",
        "DEMOMOD-PULSATION_OF_THE_TIDES",
        "DEMOMOD-SWAP_PILES",
        "DEMOMOD-SHUFFLE",
        "DEMOMOD-GLEAN_CARD",
        "DEMOMOD-CLUSTER_STRIKE",

        // 保险：兼容去下划线
        "DEMOMOD-NIGHTINGALEATTHEABYSS",
        "DEMOMOD-FORGINGATDAWN",
        "DEMOMOD-PULSATIONOFTHETIDES",
        "DEMOMOD-SWAPPILES",
        "DEMOMOD-GLEANCARD",
        "DEMOMOD-CLUSTERSTRIKE"
    ];

    private static Texture2D? _attackIcon;
    private static Texture2D? _skillIcon;
    private static Texture2D? _powerIcon;

    private static Texture2D? _attackFrame;
    private static Texture2D? _skillFrame;
    private static Texture2D? _powerFrame;
    private static Texture2D? _scoreFrame;

    // 原版 colorless 模板卡缓存
    private static CardModel? _vanillaColorlessTemplateCard;

    private static Texture2D? TryLoadTexture(params string[] paths) {
        foreach (string path in paths) {
            try {
                Texture2D? tex = ResourceLoader.Load<Texture2D>(path);
                // 被游戏 AssetCache Dispose 过的资源会留下失效实例，不能直接拿来用
                if (tex != null && GodotObject.IsInstanceValid(tex)) {
                    return tex;
                }

                Image image = new Image();
                Error err = image.Load(path);
                if (err == Error.Ok) {
                    return ImageTexture.CreateFromImage(image);
                }
            } catch (Exception e) {
            }
        }

        return null;
    }

    // 只对每个资源打一次警告，避免刷屏
    private static readonly HashSet<string> _warnedMissingAssets = [];

    /// <summary>
    /// 带有效性校验的纹理缓存。
    /// 游戏（AssetCache.UnloadMissedCacheAssets / UnloadAssets）会在切房间、回主菜单时 Dispose 掉
    /// 不在预加载集合里的资源；而 mod 的贴图不在集合内，一旦被 Dispose，
    /// 静态字段里留下的就是一个失效对象，直接赋值给 TextureRect 会表现为“图标缺省”。
    /// 所以每次使用前都要用 GodotObject.IsInstanceValid 校验，失效就重新加载。
    /// </summary>
    private static Texture2D? GetOrLoadTexture(ref Texture2D? cache, string label, params string[] paths) {
        if (cache != null && GodotObject.IsInstanceValid(cache)) {
            return cache;
        }

        cache = TryLoadTexture(paths);
        if (cache == null && _warnedMissingAssets.Add(label)) {
            Log.Warn($"[TheGleaner] 贴图加载失败，将回退成游戏默认外观：{label}（尝试过 {string.Join(" | ", paths)}）");
        }

        return cache;
    }

    private static bool IsExcluded(string cardId) {
        for (int i = 0; i < _excludedCardIds.Length; i++) {
            if (string.Equals(_excludedCardIds[i], cardId, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }

        return false;
    }

    private static bool ShouldForceVanillaColorless(string cardId) {
        for (int i = 0; i < _forceVanillaColorlessCardIds.Length; i++) {
            if (string.Equals(_forceVanillaColorlessCardIds[i], cardId, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }

        return false;
    }

    private static Texture2D? GetEnergyIcon(string type) {
        return type switch {
            "Attack" => GetOrLoadTexture(ref _attackIcon, "energy_colorless_r",
                "res://TheGleaner/images/packed/sprite_fonts/energy_colorless_r.png",
                "res://images/packed/sprite_fonts/energy_colorless_r.png"
            ),
            "Skill" => GetOrLoadTexture(ref _skillIcon, "energy_colorless_b",
                "res://TheGleaner/images/packed/sprite_fonts/energy_colorless_b.png",
                "res://images/packed/sprite_fonts/energy_colorless_b.png"
            ),
            "Power" => GetOrLoadTexture(ref _powerIcon, "energy_colorless_y",
                "res://TheGleaner/images/packed/sprite_fonts/energy_colorless_y.png",
                "res://images/packed/sprite_fonts/energy_colorless_y.png"
            ),
            _ => null
        };
    }

    private static Texture2D? GetCardFrame(string type) {
        return type switch {
            "Attack" => GetOrLoadTexture(ref _attackFrame, "card_frame_attack_s",
                "res://TheGleaner/images/packed/sprite_fonts/card_frame_attack_s.png",
                "res://images/packed/sprite_fonts/card_frame_attack_s.png"
            ),
            "Skill" => GetOrLoadTexture(ref _skillFrame, "card_frame_skill_s",
                "res://TheGleaner/images/packed/sprite_fonts/card_frame_skill_s.png",
                "res://images/packed/sprite_fonts/card_frame_skill_s.png"
            ),
            "Power" => GetOrLoadTexture(ref _powerFrame, "card_frame_power_s",
                "res://TheGleaner/images/packed/sprite_fonts/card_frame_power_s.png",
                "res://images/packed/sprite_fonts/card_frame_power_s.png"
            ),
            _ => null
        };
    }

    /// <summary>
    /// 按卡牌类型返回本 mod 的能量图标（攻击=红、技能=蓝、能力=黄）。
    /// 返回 null 表示“这张牌不归本 patch 管”，交给游戏原本的逻辑。
    /// </summary>
    internal static Texture2D? GetTypeEnergyIcon(CardModel? model) {
        if (model == null) {
            return null;
        }

        string cardId = model.Id?.Entry ?? string.Empty;
        if (!cardId.StartsWith("DEMOMOD-", StringComparison.OrdinalIgnoreCase)) {
            return null;
        }

        if (IsExcluded(cardId)) {
            return null;
        }

        // 这几张牌强制恢复成游戏原版 colorless 外观（含原版能量图标）
        if (ShouldForceVanillaColorless(cardId)) {
            return null;
        }

        return GetEnergyIcon(model.Type.ToString());
    }

    private static bool ShouldForceColorlessStatusFrame(string cardId) {
        for (int i = 0; i < _forceColorlessStatusFrameCardIds.Length; i++) {
            if (string.Equals(_forceColorlessStatusFrameCardIds[i], cardId, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }

        return false;
    }


    private static CardModel? GetVanillaColorlessTemplateCard() {
        if (_vanillaColorlessTemplateCard != null) {
            return _vanillaColorlessTemplateCard;
        }

        try {
            // 用原版 colorless 攻击牌当模板
            // FlashOfSteel / SecretWeapon / Finesse 都行，优先选攻击牌
            _vanillaColorlessTemplateCard = ModelDb.Card<FlashOfSteel>();
            if (_vanillaColorlessTemplateCard != null) {
                return _vanillaColorlessTemplateCard;
            }
        } catch (Exception e) {
        }

        try {
            _vanillaColorlessTemplateCard = ModelDb.Card<SecretWeapon>();
            if (_vanillaColorlessTemplateCard != null) {
                return _vanillaColorlessTemplateCard;
            }
        } catch (Exception e) {
        }

        try {
            _vanillaColorlessTemplateCard = ModelDb.Card<Finesse>();
            if (_vanillaColorlessTemplateCard != null) {
                return _vanillaColorlessTemplateCard;
            }
        } catch (Exception e) {
        }

        return null;
    }

    private static TextureRect? FindTextureRectByName(Node root, params string[] names) {
        for (int i = 0; i < names.Length; i++) {
            TextureRect? byUnique = root.GetNodeOrNull<TextureRect>(names[i]);
            if (byUnique != null) {
                return byUnique;
            }
        }

        foreach (Node child in root.GetChildren()) {
            if (child is TextureRect rect) {
                for (int i = 0; i < names.Length; i++) {
                    string candidate = names[i].TrimStart('%');
                    if (string.Equals(rect.Name.ToString(), candidate, StringComparison.OrdinalIgnoreCase)) {
                        return rect;
                    }
                }
            }

            TextureRect? nested = FindTextureRectByName(child, names);
            if (nested != null) {
                return nested;
            }
        }

        return null;
    }



    private static void ApplyVanillaColorlessStyle(NCard cardNode) {
        CardModel? template = GetVanillaColorlessTemplateCard();
        if (template == null) {
            return;
        }

        TextureRect? frameNode = FindTextureRectByName(cardNode, "%Frame", "Frame");
        TextureRect? energyNode = FindTextureRectByName(cardNode, "%EnergyIcon", "EnergyIcon");
        TextureRect? bannerNode = FindTextureRectByName(cardNode, "%TitleBanner", "TitleBanner", "%Banner", "Banner");
        TextureRect? portraitBorderNode = FindTextureRectByName(cardNode, "%PortraitBorder", "PortraitBorder");

        if (energyNode != null) {
            energyNode.Texture = template.EnergyIcon;
            energyNode.Visible = template.EnergyIcon != null;
        }

        if (frameNode != null) {
            frameNode.Texture = template.Frame;
            frameNode.Material = template.FrameMaterial;
            frameNode.Visible = true;
        }

        if (bannerNode != null) {
            bannerNode.Texture = template.BannerTexture;
            bannerNode.Material = template.BannerMaterial;
            bannerNode.Visible = true;
        }

        if (portraitBorderNode != null) {
            portraitBorderNode.Texture = template.PortraitBorder;
            portraitBorderNode.Material = template.BannerMaterial;
            portraitBorderNode.Visible = true;
        }
    }



    [HarmonyPostfix]
    public static void Postfix(NCard __instance) {
        try {
            var model = __instance?.Model;
            if (model == null) {
                return;
            }

            string cardId = model.Id?.Entry ?? "<null-id>";
            string type = model.Type.ToString();

            if (IsExcluded(cardId)) {
                return;
            }


            if (!cardId.StartsWith("DEMOMOD-", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            // 这 4 张牌强制改成游戏原版 colorless 外观
            if (ShouldForceVanillaColorless(cardId)) {
                ApplyVanillaColorlessStyle(__instance);
                return;
            }

            TextureRect? frameNode = __instance.GetNodeOrNull<TextureRect>("%Frame");
            if (frameNode != null) {
                string frameType = type;
                if (string.Equals(type, "Status", StringComparison.OrdinalIgnoreCase)
                    && ShouldForceColorlessStatusFrame(cardId)) {
                    frameType = "Skill";
                }

                Texture2D? frameTex = GetCardFrame(frameType);
                if (frameTex != null) {
                    frameNode.Texture = frameTex;
                }
                frameNode.Material = null;
            }

            TextureRect? energyNode = __instance.GetNodeOrNull<TextureRect>("%EnergyIcon");
            if (energyNode != null) {
                Texture2D? energyTex = GetEnergyIcon(type);
                if (energyTex != null) {
                    energyNode.Texture = energyTex;
                }
            }
        } catch (Exception e) {
        }
    }
}

[HarmonyPatch(typeof(NCard), "UpdateVisuals")]
public static class CardVisualTextPatch {
    [HarmonyPostfix]
    public static void Postfix(NCard __instance) {
        try {
            var model = __instance?.Model;
            string cardId = model?.Id?.Entry ?? string.Empty;
            if (!string.Equals(cardId, "DEMOMOD-SCORE_ENTRY_CARD", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var title = __instance.GetNodeOrNull<MegaLabel>("%TitleLabel");
            if (title != null) {
                title.SetTextAutoSize(string.Empty);
                title.Visible = false;
            }

            var energy = __instance.GetNodeOrNull<MegaLabel>("%EnergyLabel");
            if (energy != null) {
                energy.SetTextAutoSize(string.Empty);
                energy.Visible = false;
            }

        } catch (Exception e) {
        }
    }
    
}

/// <summary>
/// 模型层兜底：直接改 <see cref="CardModel.EnergyIcon"/> 的返回值。
/// 卡面（NCard.Reload 里的 _energyIcon.Texture = Model.EnergyIcon）、卡牌图鉴、
/// 详情大图（NInspectCardScreen）等所有走 Model.EnergyIcon 的地方都会拿到按类型区分的图标，
/// 不再依赖 NCard 节点时序，也不会被“节点上的贴图被后续刷新覆盖”影响。
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.EnergyIcon), MethodType.Getter)]
public static class CardEnergyIconPatch {
    [HarmonyPrefix]
    public static bool Prefix(CardModel __instance, ref Texture2D __result) {
        try {
            Texture2D? icon = CardColorPatch.GetTypeEnergyIcon(__instance);
            if (icon == null) {
                return true;
            }

            __result = icon;
            return false;
        } catch (Exception e) {
            return true;
        }
    }
}
