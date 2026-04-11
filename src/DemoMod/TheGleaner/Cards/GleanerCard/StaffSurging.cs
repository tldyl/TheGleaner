using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class StaffSurging : CustomCardModel
{
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(1),
        new PowerVar<VulnerablePower>(1),
        new PowerVar<StaffSurgingPower>(1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<StaffSurgingPower>()
    ];

    public StaffSurging() : base(2, CardType.Skill, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<WeakPower>(
            CombatState.HittableEnemies,
            DynamicVars["WeakPower"].BaseValue,
            Owner.Creature,
            this
        );

        await PowerCmd.Apply<VulnerablePower>(
            CombatState.HittableEnemies,
            DynamicVars["VulnerablePower"].BaseValue,
            Owner.Creature,
            this
        );

        await PowerCmd.Apply<StaffSurgingPower>(
            CombatState.HittableEnemies,
            DynamicVars["StaffSurgingPower"].BaseValue,
            Owner.Creature,
            this
        );
    }

    public override async Task BeforeCombatStart()
    {
        if (!IsInCombat || CombatState == null)
        {
            return;
        }

        await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WeakPower"].UpgradeValueBy(1);
    }
}