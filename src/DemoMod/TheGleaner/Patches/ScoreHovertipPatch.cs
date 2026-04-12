using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using Godot;

namespace DemoMod.TheGleaner.Patches;

[HarmonyPatch(typeof(NHoverTipSet), "Init")]
public static class ScoreKeywordHoverTipPatch
{
    // 如果你的本地化表名不是 card_keywords，只改这里
    private const string KeywordTable = "card_keywords";

    private const string ScoreTitleKey = "DEMOMOD-SCORE.title";
    private const string ScoreDescriptionKey = "DEMOMOD-SCORE.description";

    // 固定自定义 Id，用于防重复
    private const string CustomScoreTipId = "DEMOMOD-CUSTOM-SCORE-HOVERTIP";

    private static readonly string[] TriggerWords =
    {
        "乐谱",
        "拾录"
    };

    [HarmonyPrefix]
    public static void Prefix(Control owner, ref IEnumerable<IHoverTip> hoverTips)
    {
        try
        {
            if (owner is not NCardHolder holder)
            {
                return;
            }

CardModel? card = holder.CardModel;
if (card == null)
{
    return;
}

// ✅ 排除特定卡
string cardId = card.Id?.Entry ?? string.Empty;
if (string.Equals(cardId, "DEMOMOD-SCORE_ENTRY_CARD", StringComparison.OrdinalIgnoreCase))
{
    return;
}

            if (!ShouldAddScoreHoverTip(holder, card))
            {
                return;
            }

            List<IHoverTip> tips = hoverTips?.ToList() ?? new List<IHoverTip>();

            if (ContainsScoreHoverTip(tips))
            {
                hoverTips = tips;
                return;
            }

            HoverTip scoreTip = BuildScoreHoverTip();
            tips.Add(scoreTip);
            hoverTips = tips;
        }
        catch
        {
        }
    }

    private static bool ShouldAddScoreHoverTip(NCardHolder holder, CardModel card)
    {
        // 先看原始 description 本地化文本（最贴近 cards.json）
        string rawDescription = SafeGetRawDescription(card);
        if (ContainsTriggerWord(rawDescription))
        {
            return true;
        }

        // 再看实际用于当前显示的描述文本
        string renderedDescription = SafeGetRenderedDescription(holder, card);
        return ContainsTriggerWord(renderedDescription);
    }

    private static string SafeGetRawDescription(CardModel card)
    {
        try
        {
            LocString description = card.Description;
            card.DynamicVars.AddTo(description);
            return description.GetFormattedText() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string SafeGetRenderedDescription(NCardHolder holder, CardModel card)
{
    try
    {
        if (holder.CardNode == null)
        {
            return string.Empty;
        }

        Creature? target = card.CurrentTarget;
        return card.GetDescriptionForPile(holder.CardNode.DisplayingPile, target) ?? string.Empty;
    }
    catch
    {
        return string.Empty;
    }
}

    private static bool ContainsTriggerWord(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        foreach (string word in TriggerWords)
        {
            if (text.Contains(word, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsScoreHoverTip(IEnumerable<IHoverTip> tips)
    {
        foreach (IHoverTip tip in tips)
        {
            if (tip == null)
            {
                continue;
            }

            if (string.Equals(tip.Id, CustomScoreTipId, StringComparison.Ordinal))
            {
                return true;
            }

            // 兜底：如果已经有同标题/来源 tip，也不重复加
            if (tip is HoverTip textTip)
            {
                if (string.Equals(textTip.Title, GetScoreTitleSafe(), StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static HoverTip BuildScoreHoverTip()
    {
        LocString title = new LocString(KeywordTable, ScoreTitleKey);
        LocString description = new LocString(KeywordTable, ScoreDescriptionKey);

        HoverTip tip = new HoverTip(title, description)
        {
            Id = CustomScoreTipId,
            IsSmart = false,
            IsDebuff = false,
            IsInstanced = false
        };

        return tip;
    }

    private static string GetScoreTitleSafe()
    {
        try
        {
            return new LocString(KeywordTable, ScoreTitleKey).GetFormattedText() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}