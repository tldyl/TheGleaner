using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;

namespace DemoMod.TheGleaner.Relics;

[Pool(typeof(JeraRelicPool))]
public class ChronXIVGleaner : JeraExclusiveRelic {
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override string PackedIconPath => "res://TheGleaner/images/relics/demomod-chron_xi_v_gleaner.png";
    protected override string PackedIconOutlinePath => "res://TheGleaner/images/relics/demomod-chron_xi_v_gleaner.png";
    protected override string BigIconPath => "res://TheGleaner/images/relics/demomod-chron_xi_v_gleaner.png";

    private int _resonancePlayedThisTurn;
    private int _activationCountThisTurn;

    private int ResonancePlayedThisTurn {
        get => _resonancePlayedThisTurn;
        set {
            AssertMutable();
            _resonancePlayedThisTurn = value;
        }
    }

    private int ActivationCountThisTurn {
        get => _activationCountThisTurn;
        set {
            AssertMutable();
            _activationCountThisTurn = value;
            Status = _activationCountThisTurn > 0 ? RelicStatus.Active : RelicStatus.Normal;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar("Draws", 1),
        new EnergyVar(2)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(CustomEnums.Resonance)
    ];

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        CombatState combatState) {
        if (side != Owner.Creature.Side)
            return Task.CompletedTask;
        ResonancePlayedThisTurn = 0;
        ActivationCountThisTurn = 0;
        return Task.CompletedTask;
    }
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
        if (cardPlay.Card.Owner != Owner || !CombatManager.Instance.IsInProgress || ActivationCountThisTurn >= 1) {
            return;
        }
        ResonancePlayedThisTurn += cardPlay.Card.Keywords.Contains(CustomEnums.Resonance) ? 1 : 0;
        if (ResonancePlayedThisTurn <= 0) {
            return;
        }
        Flash();
        await PlayerCmd.GainEnergy(DynamicVars["Energy"].BaseValue, Owner);
        ActivationCountThisTurn++;
    }
    
    public override decimal ModifyHandDraw(Player player, decimal count) {
        if (player != Owner) {
            return count;
        }

        if (player.Creature.CombatState?.RoundNumber > 1) {
            return count;
        }

        return count + DynamicVars["Draws"].BaseValue;
    }

    public override async Task AfterCombatEnd(CombatRoom room) {
        ResonancePlayedThisTurn = 0;
        ActivationCountThisTurn = 0;
    }
}