using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Relics;

[Pool(typeof(JeraRelicPool))]
public sealed class BrittleStar : CustomRelicModel
{
    public override string PackedIconPath => $"res://TheGleaner/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string PackedIconOutlinePath => $"res://TheGleaner/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string BigIconPath => $"res://TheGleaner/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    private bool _tookDamageThisCombat;
    private bool _hasEnteredCombatSinceObtained;

    public BrittleStar()
    {
    }

    public override RelicRarity Rarity => RelicRarity.Ancient;

    [SavedProperty]
    public bool TookDamageThisCombat
    {
        get => _tookDamageThisCombat;
        set
        {
            AssertMutable();
            _tookDamageThisCombat = value;
        }
    }

    [SavedProperty]
    public bool HasEnteredCombatSinceObtained
    {
        get => _hasEnteredCombatSinceObtained;
        set
        {
            AssertMutable();
            _hasEnteredCombatSinceObtained = value;
        }
    }

    public override Task AfterObtained()
    {
        HasEnteredCombatSinceObtained = false;
        TookDamageThisCombat = false;
        return Task.CompletedTask;
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        TookDamageThisCombat = false;

        bool isCombatRoom =
            room.RoomType == RoomType.Monster ||
            room.RoomType == RoomType.Elite ||
            room.RoomType == RoomType.Boss;

        if (isCombatRoom)
        {
            HasEnteredCombatSinceObtained = true;
        }

        return Task.CompletedTask;
    }

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (Owner == null)
            return Task.CompletedTask;

        if (!HasEnteredCombatSinceObtained)
            return Task.CompletedTask;

        if (target != Owner.Creature)
            return Task.CompletedTask;

        if (result.UnblockedDamage <= 0)
            return Task.CompletedTask;

        if (props.HasFlag(ValueProp.Unblockable))
            return Task.CompletedTask;

        TookDamageThisCombat = true;
        return Task.CompletedTask;
    }

    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (Owner == null)
            return false;

        if (player != Owner)
            return false;

        if (!HasEnteredCombatSinceObtained)
            return false;

        if (room == null)
            return false;

        if (room.RoomType != RoomType.Monster &&
            room.RoomType != RoomType.Elite &&
            room.RoomType != RoomType.Boss)
        {
            return false;
        }

        if (TookDamageThisCombat)
            return false;

        rewards.Add(new RelicReward(player));
        return true;
    }
}