using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Cards.TokenCards;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class OuroborosShot : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
        new IntVar("Amount", 1)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Glean)];

    public OuroborosShot() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
        
    }

    private List<CardModel> optionCards;

    private List<CardModel> OptionCards {
        get {
            if (optionCards == null) {
                optionCards = [ModelDb.Card<Shuffle>().ToMutable(), ModelDb.Card<SwapPiles>().ToMutable(), ModelDb.Card<GleanCard>().ToMutable()];
                foreach (CardModel card in optionCards) {
                    card.Owner = Owner;
                }
            }
            return optionCards;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        AttackCommand _ = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        CardModel chosenCard = await CardSelectCmd.FromChooseACardScreen(choiceContext, OptionCards, Owner);
        if (chosenCard != null) {
            await ((IChoosable) chosenCard).OnChosen(choiceContext, cardPlay);
        }
        await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
        NCard cardNode = NCombatRoom.Instance?.Ui.GetCardFromPlayContainer(this);
        Tween tween = NCombatRoom.Instance.CreateTween().SetParallel();
        //tween.Chain().TweenCallback(Callable.From((Action) (() => NCombatRoom.Instance.Ui.AddChildSafely((Node) NExhaustVfx.Create(cardNode)))));
        tween.Parallel().TweenProperty(cardNode, (NodePath) "modulate", StsColors.exhaustGray, SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast ? 0.2 : 0.3);
        tween.Chain().TweenCallback(Callable.From(cardNode.QueueFreeSafely));
    }
    
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}
