using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
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
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using MegaCrit.Sts2.Core.Models;



namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Bisbigliando : CustomCardModel
{
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(5),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature _) =>
            -CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e) =>
                e.HappenedThisTurn(card.CombatState)
                && e.CardPlay.Card.Id.Entry == card.Id.Entry
                && e.CardPlay.Card.Owner == card.Owner))
    ];

    public Bisbigliando() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);

        NCard cardNode = NCombatRoom.Instance?.Ui.GetCardFromPlayContainer(this);
        if (cardNode != null)
        {
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

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(2);
    }
}