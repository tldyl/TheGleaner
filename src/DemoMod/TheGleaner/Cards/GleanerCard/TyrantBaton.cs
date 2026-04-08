using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class TyrantBaton : CustomCardModel, IConcertoCard {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Concerto)];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar("Draw", 1)
    ];

    public TyrantBaton() : base(3, CardType.Skill, CardRarity.Rare, TargetType.RandomEnemy) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        foreach (CardModel card in Owner.PlayerCombatState.Hand.Cards.Where(c => c is IConcertoCard and not TyrantBaton).ToList()) {
            await CardCmd.AutoPlay(choiceContext, card, null);
        }
    }
    
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);

    public async Task OnConcerto(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await CardPileCmd.Draw(choiceContext, DynamicVars["Draw"].BaseValue, Owner);
    }
}
