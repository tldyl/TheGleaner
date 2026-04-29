using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Enums;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Utils;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.ValueProps;

namespace DemoMod.TheGleaner.Cards.GleanerCard;
[Pool(typeof(CardPool))]
public class Bisbigliando : CustomCardModel {
    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(2, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CustomEnums.Score)];

    public Bisbigliando() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
        GleanerVfxCmd.PlayOnCreature<Node2D>(cardPlay.Target, "res://TheGleaner/scenes/vfx/arrow_attack.tscn", 0.3f);
        await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", 0.5f);
        GleanerVfxCmd.PlayOnCreature<Node2D>(cardPlay.Target, "res://TheGleaner/scenes/vfx/arrow_hit_vfx.tscn");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .WithNoAttackerAnim()
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

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

    protected override void OnUpgrade() {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
