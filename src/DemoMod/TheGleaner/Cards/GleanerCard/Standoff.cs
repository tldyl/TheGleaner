using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Standoff : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new List<DynamicVar> { new DamageVar(11, ValueProp.Move) };

    public Standoff() : base(0, CardType.Attack, CardRarity.Common, TargetType.AllEnemies, true, true) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        IEnumerable<Creature> allEnemies = CombatState.HittableEnemies.Where(enemy => enemy.Monster.IntendsToAttack);
        await CreatureCmd.Damage(choiceContext, allEnemies, DynamicVars.Damage, Owner.Creature, this);
    }

    protected override void OnUpgrade() {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
