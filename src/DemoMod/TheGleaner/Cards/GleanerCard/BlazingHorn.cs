using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class BlazingHorn : CustomCardModel, IConcertoCard {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(3),
        new EnergyVar("Energy2", 1)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Concerto)];

    public BlazingHorn() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        SoundManager.Instance.PlaySound(SoundKeys.GetSoundResourcePath("HORN_" + new Random().Next(1, 5)), 1.0f);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
    }
    
    public async Task OnConcerto(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        SoundManager.Instance.PlaySound(SoundKeys.GetSoundResourcePath("HORN_" + new Random().Next(1, 5)), 1.0f);
        await PowerCmd.Apply<EnergyNextTurnPower>(Owner.Creature, DynamicVars["Energy2"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Energy.UpgradeValueBy(1);
}
