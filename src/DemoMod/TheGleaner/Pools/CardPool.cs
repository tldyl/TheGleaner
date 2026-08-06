using BaseLib.Abstracts;
using DemoMod.TheGleaner.Cards.GleanerCard;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace DemoMod.TheGleaner.Pools;

public class CardPool : CustomCardPoolModel {
    public override string Title => "Gleaner Card Pool";
    public override string EnergyColorName => "ironclad";
    public override Color DeckEntryCardColor => new Color(0.5f, 0.5f, 1f);
    public override bool IsColorless => false;

    public override Texture2D? CustomFrame(CustomCardModel card) {
        if (card is ScoreEntryCard) {
            return PreloadManager.Cache.GetTexture2D("res://TheGleaner/images/packed/sprite_fonts/Score.png");
        }
        switch (card.Type) {
            case CardType.Attack:
                return PreloadManager.Cache.GetTexture2D("res://TheGleaner/images/packed/sprite_fonts/card_frame_attack_s.png");
            case CardType.Skill:
                return PreloadManager.Cache.GetTexture2D("res://TheGleaner/images/packed/sprite_fonts/card_frame_skill_s.png");
            default:
                return PreloadManager.Cache.GetTexture2D("res://TheGleaner/images/packed/sprite_fonts/card_frame_power_s.png");
        }
    }
}
