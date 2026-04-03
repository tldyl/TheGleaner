using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Phantasm : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new List<DynamicVar> {
            new BlockVar(6, ValueProp.Move),
            new IntVar("BlockTimes", 3),
            new EnergyVar(1)
        };

    public Phantasm() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true, true) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        for (int i = 0; i < DynamicVars["BlockTimes"].IntValue; i++) {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }
    }

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side) {
        CardPile scorePile = CustomPiles.GetCustomPile(Owner.PlayerCombatState, CustomEnums.ScorePile);
        if (scorePile != null && scorePile.Cards.Contains(this)) {
            EnergyCost.AddUntilPlayed(-1);
            return;
        }

        if (Owner.PlayerCombatState.Hand.Cards.Contains(this)) {
            await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
        }
    }

    protected override void OnUpgrade() {
        DynamicVars["BlockTimes"].UpgradeValueBy(1);
    }
}
