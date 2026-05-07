using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Relics;

[Pool(typeof(JeraRelicPool))]
public sealed class IcedBlueberry : CustomRelicModel
{
    public override string PackedIconPath => $"res://TheGleaner/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string PackedIconOutlinePath => $"res://TheGleaner/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string BigIconPath => $"res://TheGleaner/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    private const string TurnsKey = "Turns";

    private int _cooldown;

    public IcedBlueberry()
    {
    }

    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new DynamicVar(TurnsKey, 2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.ForEnergy(this)
    ];

    public override bool ShowCounter => DisplayAmount > 0;

    public override int DisplayAmount
    {
        get
        {
            if (!CombatManager.Instance.IsInProgress)
                return -1;

            if (IsCanonical)
                return -1;

            if (Cooldown <= 0)
                return -1;

            return Cooldown;
        }
    }

    private int Cooldown
    {
        get => _cooldown;
        set
        {
            AssertMutable();
            _cooldown = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (Owner == null)
            return amount;

        if (player != Owner)
            return amount;

        if (Cooldown > 0)
            return amount;

        return amount + DynamicVars.Energy.IntValue;
    }

    public override Task AfterObtained()
    {
        Cooldown = 0;
        Status = RelicStatus.Active;
        return Task.CompletedTask;
    }

    public override Task BeforeCombatStart()
    {
        Cooldown = 0;
        Status = RelicStatus.Active;
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

        if (target != Owner.Creature)
            return Task.CompletedTask;

        if (result.UnblockedDamage <= 0)
            return Task.CompletedTask;

        if (props.HasFlag(ValueProp.Unblockable))
            return Task.CompletedTask;

        if (Cooldown > 0)
            return Task.CompletedTask;

        Cooldown = DynamicVars[TurnsKey].IntValue;
        Status = RelicStatus.Disabled;
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (Owner == null)
            return;

        if (side != Owner.Creature.Side)
            return;

        if (Cooldown > 0)
        {
            Cooldown--;

            if (Cooldown <= 0)
            {
                Status = RelicStatus.Active;
                InvokeDisplayAmountChanged();
                Flash();

                await PlayerCmd.GainEnergy(1m, Owner);
            }

            return;
        }
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        Status = RelicStatus.Normal;
        Cooldown = 0;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }
}