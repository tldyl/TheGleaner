using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Cards.TokenCards;
using DemoMod.TheGleaner.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace DemoMod.TheGleaner.Cards.TokenCards;

[Pool(typeof(TokenCardPool))]
public class ThreeOrMoreCostAttacks : CustomCardModel, IChoosable {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    public override bool CanBeGeneratedInCombat => false;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

    public ThreeOrMoreCostAttacks() : base(-1, CardType.Skill, CardRarity.Token, TargetType.None) {
        
    }

    public async Task OnChosen(PlayerChoiceContext choiceContext, CardPlay cardPlay, params object[] extraParams) {
        int addAmount = 3;
        if (extraParams is {Length: > 0}) {
            addAmount = (int)extraParams[0];
        }
        List<CardPoolModel> list1 = Owner.UnlockState.CharacterCardPools.ToList();
        if (list1.Count > 1) {
            list1.Remove(Owner.Character.CardPool);
        }
        IEnumerable<CardModel> cards = from c in list1.SelectMany(c => c.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint))
            where c.Type == CardType.Attack && c.EnergyCost.Canonical >= 3 && !c.HasStarCostX && !c.Tags.Contains(CardTag.OstyAttack)
            select c;
        List<CardModel> list2 = CardFactory.GetDistinctForCombat(Owner, cards, addAmount, Owner.RunState.Rng.CombatCardGeneration).ToList();
        if (IsUpgraded) {
            foreach (CardModel card2 in list2) {
                CardCmd.Upgrade(card2);
            }
        }
        foreach (CardModel c in list2) {
            CardCmd.Preview(c);
            await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, c);
        }
    }
}
