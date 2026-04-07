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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class PhantomHarp : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new List<DynamicVar> {
            new DamageVar(7, ValueProp.Move),
            new RepeatVar(3),
            new EnergyVar(1)
        };

    public PhantomHarp() : base(3, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true, true) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side) {
        CardPile scorePile = CustomPiles.GetCustomPile(Owner.PlayerCombatState, CustomEnums.ScorePile);
        if (scorePile != null && scorePile.Cards.Contains(this) && side == CombatSide.Player) {
            EnergyCost.AddUntilPlayed(-1);
            return;
        }

        if (Owner.PlayerCombatState.Hand.Cards.Contains(this)) {
            await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
        }
    }

    protected override void OnUpgrade() {
        DynamicVars.Repeat.UpgradeValueBy(1);
    }
}
