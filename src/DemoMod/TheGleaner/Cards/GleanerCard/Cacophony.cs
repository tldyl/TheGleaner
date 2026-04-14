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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Cacophony : CustomCardModel
{
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(8, ValueProp.Move),
        new PowerVar<WeakPower>(1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(CustomEnums.Dissonance),
        HoverTipFactory.FromPower<WeakPower>()
    ];

    public Cacophony() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Damage(
            choiceContext,
            CombatState.HittableEnemies,
            DynamicVars.Damage,
            Owner.Creature,
            this
        );

        await PowerCmd.Apply<WeakPower>(
            CombatState.HittableEnemies,
            DynamicVars["WeakPower"].BaseValue,
            Owner.Creature,
            this
        );

        List<CardModel> cards = RandomDissonanceCard.getRandomDissonanceCards(
            1,
            Owner.RunState.Rng.CombatCardGeneration
        );

        foreach (CardModel card in cards)
        {
            PileType targetPile =
                Owner.RunState.Rng.CombatCardGeneration.NextInt(2) == 0
                    ? PileType.Draw
                    : PileType.Discard;

            IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(
                [CombatState.CreateCard(card, Owner)],
                targetPile,
                true,
                CardPilePosition.Random
            );

            CardCmd.PreviewCardPileAdd(results, 1.2f, CardPreviewStyle.HorizontalLayout);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}