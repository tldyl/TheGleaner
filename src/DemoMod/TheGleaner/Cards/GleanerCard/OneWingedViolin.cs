using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class OneWingedViolin : CustomCardModel, IConcertoCard {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Amount", 2)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromKeyword(CustomEnums.Concerto)
    ];

    public OneWingedViolin() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await PowerCmd.Apply<OneWingedViolinPower>(Owner.Creature, 1, Owner.Creature, this);
    }

    public async Task OnConcerto(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await PowerCmd.Apply<StrengthPower>(Owner.Creature, DynamicVars["Amount"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(Owner.Creature, DynamicVars["Amount"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<FlexPotionPower>(Owner.Creature, DynamicVars["Amount"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<SpeedPotionPower>(Owner.Creature, DynamicVars["Amount"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Amount"].BaseValue += 1;
}
