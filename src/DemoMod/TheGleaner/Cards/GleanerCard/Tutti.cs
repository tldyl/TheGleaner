using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.CardPiles;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Tutti : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Score)];

    public Tutti() : base(3, CardType.Skill, CardRarity.Rare, TargetType.RandomEnemy) {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        ScorePile scorePile = ScorePileCmd.GetOrCreateScorePile(Owner.PlayerCombatState);
        foreach (CardModel card in scorePile.Cards.Reverse()) {
            await CardCmd.AutoPlay(choiceContext, card, null);
        }
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
