using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class PaperFrost : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new IntVar("Amount", 4)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public PaperFrost() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        if (CurrentUpgradeLevel > 0) {
            await PowerCmd.Apply<StrengthPower>(
                CombatState.HittableEnemies,
                -DynamicVars["Amount"].BaseValue,
                Owner.Creature,
                this,
                false
            );
        } else {
            await PowerCmd.Apply<StrengthPower>(
                cardPlay.Target,
                -DynamicVars["Amount"].BaseValue,
                Owner.Creature,
                this,
                false
            );
        }
    }

    public override async Task BeforeCombatStart() {
        if (!IsInCombat || CombatState == null) {
            return;
        }

        await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
    }

    protected override void OnUpgrade() {
        AccessTools.Field(typeof(CardModel), "<TargetType>k__BackingField").SetValue(this, TargetType.AllEnemies);
    }
}