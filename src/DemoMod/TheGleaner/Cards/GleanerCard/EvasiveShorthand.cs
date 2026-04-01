using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class EvasiveShorthand : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Amount", 1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Glean)];

    public EvasiveShorthand() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        await ScorePileCmd.Glean(Owner, choiceContext, DynamicVars["Amount"].BaseValue, this);
    }

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side) {
        if (Owner.PlayerCombatState.Hand.Cards.Contains(this)) {
            await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
        }
    }

    protected override void OnUpgrade() => DynamicVars["Amount"].UpgradeValueBy(1);
}

