using BaseLib.Abstracts;
using BaseLib.Utils;
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
public class ZeroCostAttacks : CustomCardModel, IChoosable {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    public override bool CanBeGeneratedInCombat => false;
    private List<DynamicVar> _dynamicVars = [];

    public ZeroCostAttacks() : base(-1, CardType.Skill, CardRarity.Token, TargetType.None) {
        
    }

    public void addVar(DynamicVar dynamicVar) {
        _dynamicVars.Add(dynamicVar);
    }
    
    public async Task OnChosen(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        List<CardPoolModel> list1 = Owner.UnlockState.CharacterCardPools.ToList();
        if (list1.Count > 1) {
            list1.Remove(Owner.Character.CardPool);
        }
        IEnumerable<CardModel> cards = from c in list1.SelectMany(c => c.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint))
            where c.Type == CardType.Attack && c.EnergyCost.Canonical == 0
            select c;
        List<CardModel> list2 = CardFactory.GetDistinctForCombat(Owner, cards, _dynamicVars[0].IntValue, Owner.RunState.Rng.CombatCardGeneration).ToList();
        if (IsUpgraded) {
            foreach (CardModel card2 in list2) {
                CardCmd.Upgrade(card2);
            }
        }
        foreach (CardModel c in list2) {
            await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, c);
        }
        foreach (CardModel c in list2) {
            CardCmd.Preview(c);
        }
    }
}
