using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class ShriekOfDread : CustomCardModel, IDissonanceCard {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("VulVal", 2)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>(), HoverTipFactory.FromCard<NightingaleAtTheAbyss>()];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal, CardKeyword.Exhaust, CustomEnums.Dissonance];

    public ShriekOfDread() : base(1, CardType.Status, CardRarity.Status, TargetType.AllEnemies) {
        
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
    }
    
    public override async Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal) {
        if (card == this) {
            foreach (Creature creature in Owner.Creature.CombatState.Creatures) {
                await PowerCmd.Apply<VulnerablePower>(creature, DynamicVars["VulVal"].BaseValue, Owner.Creature, this);
            }
        }
    }

    public void OnEnterScorePile(PlayerCombatState combatState, Player player) {
        CardModel card = Owner.Creature.CombatState.CreateCard<NightingaleAtTheAbyss>(Owner);
        TaskHelper.RunSafely(CardCmd.Transform(this, card));
    }
}
