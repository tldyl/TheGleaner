using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Commands;

using MegaCrit.Sts2.Core.Models;


using CustomEnums = DemoMod.TheGleaner.Enums.CustomEnums;


namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Improvisation : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(3)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Innate,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(CustomEnums.Glean)
    ];

    public Improvisation() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        IEnumerable<CardModel> cards = CardFactory.GetForCombat(
            Owner,
            Owner.Character.CardPool.GetUnlockedCards(
                Owner.UnlockState,
                Owner.RunState.CardMultiplayerConstraint
            ),
            DynamicVars.Cards.IntValue,
            Owner.RunState.Rng.CombatCardGeneration
        );

        foreach (CardModel card in cards) {
            if (IsUpgraded) {
                CardCmd.Upgrade(card, CardPreviewStyle.HorizontalLayout);
            }

            await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, card);
        }
    }
}