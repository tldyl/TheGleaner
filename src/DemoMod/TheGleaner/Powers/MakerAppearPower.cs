using BaseLib.Abstracts;
using DemoMod.TheGleaner.Monsters;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Powers;

public class MakerAppearPower : CustomPowerModel {
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar("DisplayAmount", 0)
    ];
    public override int DisplayAmount => DynamicVars["DisplayAmount"].IntValue;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource) {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, 1, Owner, null);
        switch (Owner.Monster) {
            case ScrutinyDoor:
                NCombatRoom.Instance.GetCreatureNode(Owner).Visuals.GetNode<Sprite2D>("%Visuals").Texture = PreloadManager.Cache.GetTexture2D("res://TheGleaner/images/monsters/door_maker_placeholder_2.png");
                break;
            case HungerDoor:
                NCombatRoom.Instance.GetCreatureNode(Owner).Visuals.GetNode<Sprite2D>("%Visuals").Texture = PreloadManager.Cache.GetTexture2D("res://TheGleaner/images/monsters/door_maker_placeholder_3.png");
                break;
            case GraspDoor:
                NCombatRoom.Instance.GetCreatureNode(Owner).Visuals.GetNode<Sprite2D>("%Visuals").Texture = PreloadManager.Cache.GetTexture2D("res://TheGleaner/images/monsters/door_maker_placeholder_4.png");
                break;
        }
    }

    public override async Task AfterRemoved(Creature oldOwner) {
        if (Owner.Monster is ScrutinyDoor or HungerDoor or GraspDoor) {
            NCombatRoom.Instance.GetCreatureNode(Owner).Visuals.GetNode<Sprite2D>("%Visuals").Texture = PreloadManager.Cache.GetTexture2D("res://TheGleaner/images/monsters/door_maker_placeholder_1.png");
        }
    }
    
    public override Decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource) {
        return target != Owner ? 1 : 2;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
        DynamicVars["DisplayAmount"].BaseValue++;
        if (DynamicVars["DisplayAmount"].BaseValue >= 3) {
            DynamicVars["DisplayAmount"].BaseValue = 0;
            if (Owner.CombatState.HittableEnemies.Count > 1) {
                int index = Owner.CombatState.HittableEnemies.IndexOf(Owner);
                index++;
                index %= Owner.CombatState.HittableEnemies.Count;
                await PowerCmd.Remove(this);
                PowerModel cpy = (PowerModel) MutableClone();
                //AccessTools.Field(typeof(PowerModel), "_owner").SetValue(cpy, null);
                await PowerCmd.Apply(context, cpy, Owner.CombatState.HittableEnemies[index], 1, Owner, null);
            }
        }
        InvokeDisplayAmountChanged();
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength) {
        if (creature == Owner) {
            if (Owner.CombatState.HittableEnemies.Count > 1) {
                int index = Owner.CombatState.HittableEnemies.IndexOf(Owner);
                index++;
                index %= Owner.CombatState.HittableEnemies.Count;
                await PowerCmd.Remove(this);
                PowerModel cpy = (PowerModel) MutableClone();
                //AccessTools.Field(typeof(PowerModel), "_owner").SetValue(cpy, null);
                await PowerCmd.Apply(choiceContext, cpy, Owner.CombatState.HittableEnemies[index], 1, Owner, null);
            }
        }
    }
}
