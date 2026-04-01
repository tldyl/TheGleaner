using BaseLib.Abstracts;
using DemoMod.TheGleaner.Cards.GleanerCard;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using DemoMod.TheGleaner.Pools;
using DemoMod.TheGleaner.Relics;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace DemoMod.TheGleaner.Characters;

public class TheGleaner : PlaceholderCharacterModel {
    public override float AttackAnimDelay => 0.5f;
    public override float CastAnimDelay => 0.5f;
    public override Color NameColor => new Color(0.5f, 0.5f, 1f);
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 74;
    
    // 人物模型tscn路径。要自定义见下。
    public override string CustomVisualPath => "res://TheGleaner/scenes/gleaner_character.tscn";
    // 卡牌拖尾路径。
    // public override string CustomTrailPath => "res://scenes/vfx/card_trail_ironclad.tscn";
    // 人物头像路径。
    public override string CustomIconTexturePath => "res://TheGleaner/images/character_icon_gleaner.png";
    // 人物头像2号。
    public override string CustomIconPath => "res://TheGleaner/scenes/gleaner_icon.tscn";
    // 能量表盘tscn路径。要自定义见下。
    public override string CustomEnergyCounterPath => "res://TheGleaner/scenes/gleaner_energy_counter.tscn";
    // 篝火休息动画。
    // public override string CustomRestSiteAnimPath => "res://scenes/rest_site/characters/ironclad_rest_site.tscn";
    // 商店人物动画。
    // public override string CustomMerchantAnimPath => "res://scenes/merchant/characters/ironclad_merchant.tscn";
    // 多人模式-手指。
    // public override string CustomArmPointingTexturePath => null;
    // 多人模式剪刀石头布-石头。
    // public override string CustomArmRockTexturePath => null;
    // 多人模式剪刀石头布-布。
    // public override string CustomArmPaperTexturePath => null;
    // 多人模式剪刀石头布-剪刀。
    // public override string CustomArmScissorsTexturePath => null;

    // 人物选择背景。
    public override string CustomCharacterSelectBg => "res://TheGleaner/scenes/gleaner_bg.tscn";
    // 人物选择图标。
    public override string CustomCharacterSelectIconPath => "res://TheGleaner/images/char_select_gleaner.png";
    // 人物选择图标-锁定状态。
    public override string CustomCharacterSelectLockedIconPath => "res://TheGleaner/images/char_select_gleaner_locked.png";
    // 人物选择过渡动画。
    // public override string CustomCharacterSelectTransitionPath => "res://materials/transitions/ironclad_transition_mat.tres";
    // 地图上的角色标记图标、表情轮盘上的角色头像
    // public override string CustomMapMarkerPath => null;
    // 攻击音效
    // public override string CustomAttackSfx => null;
    // 施法音效
    // public override string CustomCastSfx => null;
    // 死亡音效
    // public override string CustomDeathSfx => null;
    // 角色选择音效
    // public override string CharacterSelectSfx => null;
    // 过渡音效。这个不能删。
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";
    
    public override CardPoolModel CardPool => ModelDb.CardPool<CardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<JeraRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<PotionPool>();
    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<StrikeGleaner>(),
        ModelDb.Card<StrikeGleaner>(),
        ModelDb.Card<StrikeGleaner>(),
        ModelDb.Card<StrikeGleaner>(),
        ModelDb.Card<DefendGleaner>(),
        ModelDb.Card<DefendGleaner>(),
        ModelDb.Card<DefendGleaner>(),
        ModelDb.Card<DefendGleaner>(),
        ModelDb.Card<Glissando>(),
        ModelDb.Card<StaffSurging>(),
        ModelDb.Card<StringAndPillar>()
    ];
    public override IReadOnlyList<RelicModel> StartingRelics => [
        ModelDb.Relic<Jera>()
    ];

    public override List<string> GetArchitectAttackVfx() => [
        "vfx/vfx_attack_blunt",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_attack_slash",
        "vfx/vfx_bloody_impact",
        "vfx/vfx_rock_shatter"
    ];

    public override CreatureAnimator GenerateAnimator(MegaSprite controller) {
        AnimState animState = new AnimState("idle", true);
        AnimState state1 = new AnimState("zhouyin"); //cast
        AnimState state2 = new AnimState("shuntan"); //attack
        AnimState state3 = new AnimState("zhouyin"); //hurt
        AnimState state4 = new AnimState("idle"); //die
        AnimState state5 = new AnimState("idle", true); //relaxed_loop
        state1.NextState = animState;
        state2.NextState = animState;
        state3.NextState = animState;
        state5.AddBranch("Idle", animState);
        CreatureAnimator animator = new CreatureAnimator(animState, controller);
        animator.AddAnyState("Idle", animState);
        animator.AddAnyState("Dead", state4);
        animator.AddAnyState("Hit", state3);
        animator.AddAnyState("Attack", state2);
        animator.AddAnyState("Cast", state1);
        animator.AddAnyState("Relaxed", state5);
        return animator;
    }
}
