using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using System.Linq;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Boiling : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    private readonly List<CardModel> _affectedCards = [];
    private bool _costIncreaseActive;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar("EnergyAmount", 2),
        new EnergyVar("Amount", 1)
    ];

    public Boiling() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await PlayerCmd.GainEnergy(DynamicVars["EnergyAmount"].BaseValue, Owner);

        _affectedCards.Clear();
        foreach (CardModel card in Owner.PlayerCombatState.Hand.Cards.ToList()) {
            card.EnergyCost.AddThisCombat(1);
            _affectedCards.Add(card);
        }

        _costIncreaseActive = _affectedCards.Count > 0;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
        if (!_costIncreaseActive || cardPlay.Card.Owner != Owner || cardPlay.Card.Type != CardType.Attack) {
            return;
        }

        foreach (CardModel card in _affectedCards.Where(c => c.CombatState != null)) {
            card.EnergyCost.AddThisCombat(-1);
        }

        _affectedCards.Clear();
        _costIncreaseActive = false;
        await Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars["EnergyAmount"].UpgradeValueBy(1);
}