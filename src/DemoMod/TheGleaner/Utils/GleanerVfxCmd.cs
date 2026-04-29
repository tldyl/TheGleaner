using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Cards.GleanerCard;
using DemoMod.TheGleaner.Commands;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.TestSupport;

namespace DemoMod.TheGleaner.Utils;

public class GleanerVfxCmd {
    public static T PlayOnCreature<T>(Creature target, string path, float delay = 0) where T : Node2D {
        T vfx = null;
        if (!TestMode.IsOn && NCombatRoom.Instance != null && !target.IsDead) {
            NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(target);
            if (creatureNode != null) {
                vfx = PlayVfx<T>(creatureNode.Visuals.GetNode<Marker2D>("%CenterPos").GlobalPosition, path, delay);
            }
        }
        return vfx;
    }

    public static void PlayVfx<T>(Vector2 position, T vfx, float delay = 0) where T : Node2D {
        if (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) {
            delay /= 2.0f;
            delay = Math.Min(delay, 0.25f);
        }
        if (!TestMode.IsOn && NCombatRoom.Instance != null) {
            if (delay > 0) {
                Godot.Timer timer = new Godot.Timer
                {
                    WaitTime = delay,
                    OneShot = true,
                    Autostart = true
                };
                NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(timer);

                void Action() {
                    NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                    vfx.GlobalPosition = position;
                    timer.Timeout -= Action;
                    NCombatRoom.Instance.CombatVfxContainer.RemoveChildSafely(timer);
                }

                timer.Timeout += Action;
            } else {
                NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                vfx.GlobalPosition = position;
            }
        }
    }
    
    public static T PlayVfx<T>(Vector2 position, string path, float delay = 0) where T : Node2D {
        T vfx = PreloadManager.Cache.GetScene(path).Instantiate<T>();
        PlayVfx(position, vfx, delay);
        return vfx;
    }

    public static void CheckScoreIsEmpty(PlayerCombatState playerCombatState) {
        ScorePile scorePile = ScorePileCmd.GetOrCreateScorePile(playerCombatState);
        NHandCardHolder holder = NRun.Instance.CombatRoom.Ui.Hand.ActiveHolders.FirstOrDefault(holder => holder.CardModel is ScoreEntryCard);
        if (holder == null) {
            return;
        }
        CardModel card = holder.CardModel;
        if (scorePile.Cards.Count == 0) {
            ScorePileCmd.hasScoreEntryCard.Set(card.Owner, false);
            NCombatRoom.Instance.GetCreatureNode(card.Owner.Creature).GetNode<Node2D>("ScoreOpenVfx").Visible = false;
            if (!LocalContext.IsMe(card.Owner)) {
                return;
            }
            NRun.Instance.CombatRoom.Ui.Hand.Remove(card);
        } else {
            NRun.Instance.CombatRoom.Ui.Hand.ForceRefreshCardIndices();
        }
    }
}
