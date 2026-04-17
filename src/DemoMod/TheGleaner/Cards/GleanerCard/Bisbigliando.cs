using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Bisbigliando : CustomCardModel
{
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override bool ShouldGlowGoldInternal => !HasBeenPlayedThisTurn;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5, ValueProp.Move)
    ];

    public Bisbigliando() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        if (!HasBeenPlayedThisTurn) {
            await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);

            NCard cardNode = NCombatRoom.Instance?.Ui.GetCardFromPlayContainer(this);
            if (cardNode != null) {
                Tween tween = NCombatRoom.Instance.CreateTween().SetParallel();
                tween.Parallel().TweenProperty(
                    cardNode,
                    (NodePath)"modulate",
                    StsColors.exhaustGray,
                    SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast ? 0.2 : 0.3
                );
                tween.Chain().TweenCallback(Callable.From(cardNode.QueueFreeSafely));
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }

    private bool HasBeenPlayedThisTurn
    {
        get
        {
            return CombatManager.Instance.History.CardPlaysFinished.Any(
                (CardPlayFinishedEntry e) =>
                    e.CardPlay.Card == this &&
                    e.HappenedThisTurn(CombatState)
            );
        }
    }
}