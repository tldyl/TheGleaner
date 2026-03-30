using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class TwiningTone : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar("Amount", 2),
        new DamageVar(6, ValueProp.Move)
    ];

    public TwiningTone() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        CardPile scorePile = CustomPiles.GetCustomPile(Owner.PlayerCombatState, CustomEnums.ScorePile);
        decimal bonusDamage = (scorePile?.Cards.Count ?? 0) * DynamicVars["Amount"].BaseValue;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonusDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars["Amount"].UpgradeValueBy(1);
}
