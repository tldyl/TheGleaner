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
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(TokenCardPool))]
public class RoundAndRound : CustomCardModel {
    //public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [StunIntent.GetStaticHoverTip()];

    public RoundAndRound() : base(0, CardType.Skill, CardRarity.Common, TargetType.AllEnemies) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        foreach (Creature creature in Owner.Creature.CombatState.HittableEnemies) {
            await CreatureCmd.Stun(creature);
        }
        if (CurrentUpgradeLevel > 0) {
            await PowerCmd.Apply<EnergyNextTurnPower>(Owner.Creature, Owner.PlayerCombatState.MaxEnergy, Owner.Creature, this);
        }
    }
}
