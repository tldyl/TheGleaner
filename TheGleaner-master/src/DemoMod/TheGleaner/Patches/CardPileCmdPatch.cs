using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;

namespace DemoMod.TheGleaner.Patches;
public class CardPileCmdPatch {
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
                ThinkCmd.Play(new LocString("combat_messages", "JERA_HAND_FULL"), player.Creature, 2.0);
            }
            return player.Character is not Characters.TheGleaner;
        }
    }
}
