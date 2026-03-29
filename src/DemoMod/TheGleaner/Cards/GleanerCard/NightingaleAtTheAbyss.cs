using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class NightingaleAtTheAbyss : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("VulVal", 2),
        new IntVar("WeakVal", 1),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>(), HoverTipFactory.FromPower<WeakPower>()];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public NightingaleAtTheAbyss() : base(1, CardType.Skill, CardRarity.Event, TargetType.AllEnemies) {
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        foreach (Creature creature in Owner.Creature.CombatState.Enemies) {
            await PowerCmd.Apply<VulnerablePower>(creature, DynamicVars["VulVal"].BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<WeakPower>(creature, DynamicVars["WeakVal"].BaseValue, Owner.Creature, this);
        }
    }
    
    protected override void OnUpgrade() {
        DynamicVars["VulVal"].UpgradeValueBy(1);
        DynamicVars["WeakVal"].UpgradeValueBy(1);
    }
}
