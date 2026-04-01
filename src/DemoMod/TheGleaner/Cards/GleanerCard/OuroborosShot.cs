using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Cards.TokenCards;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class OuroborosShot : CustomCardModel {
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
        new IntVar("Amount", 1)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Glean)];

    public OuroborosShot() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        CardModel chosenCard = await CardSelectCmd.FromChooseACardScreen(choiceContext, [ModelDb.Card<Shuffle>(), ModelDb.Card<SwapPiles>(), ModelDb.Card<GleanCard>()], Owner);
        if (chosenCard != null) {
            await ((IChoosable) chosenCard).OnChosen(choiceContext, cardPlay);
        }
        await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
    }
    
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}
