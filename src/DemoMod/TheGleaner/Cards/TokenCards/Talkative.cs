using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using DemoMod.TheGleaner.Monsters;

namespace DemoMod.TheGleaner.Cards.TokenCards;

[Pool(typeof(TokenCardPool))]
public class Talkative : CustomCardModel {
    //public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(1),
        new PowerVar<StrengthPower>(1)
    ];
    
    public override int MaxUpgradeLevel => 0;

    public Talkative() : base(1, CardType.Status, CardRarity.Status, TargetType.Self) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await PowerCmd.Apply<WeakPower>(Owner.Creature, DynamicVars["WeakPower"].BaseValue, Owner.Creature, this);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) {
        if (Owner.PlayerCombatState.Hand.Cards.Contains(this) && cardPlay.Card.Owner == Owner && cardPlay.Card != this) {
            foreach (Creature creature in Owner.Creature.CombatState.HittableEnemies) {
                if (creature is {IsMonster: true, Monster: SirenCultist}) {
                    await PowerCmd.Apply<StrengthPower>(creature, DynamicVars["StrengthPower"].BaseValue, Owner.Creature, this);
                }
            }
        }
    }
}
