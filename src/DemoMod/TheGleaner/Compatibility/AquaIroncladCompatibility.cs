using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace AquaIroncladMod;

public abstract class MyCharacterCard : CustomCardModel {
    protected MyCharacterCard(
        int cost,
        CardType type,
        CardRarity rarity,
        TargetType targetType,
        bool exhaust = false,
        bool ethereal = false
    ) : base(cost, type, rarity, targetType, exhaust, ethereal) {
    }

    public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
}

public abstract class MyCharacterRelic : CustomRelicModel {
    protected MyCharacterRelic(string legacyId) {
    }
}

public static class FileLogger {
    public static void Write(string message) {
        GD.Print($"[AquaIronclad] {message}");
    }
}
