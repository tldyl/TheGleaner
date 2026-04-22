using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace DemoMod.TheGleaner.Potions;

/// <summary>
/// 点金药水：将当前金币翻倍。
/// </summary>
[Pool(typeof(PotionPool))]
public sealed class AlchemyGoldPotion : CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Common;

    public override PotionUsage Usage => PotionUsage.AnyTime;

    public override TargetType TargetType => TargetType.None;

    protected override Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (Owner == null)
            return Task.CompletedTask;

        int currentGold = Owner.Gold;

        if (currentGold <= 0)
            return Task.CompletedTask;

        long doubled = (long)currentGold * 2L;
        Owner.Gold = doubled > int.MaxValue ? int.MaxValue : (int)doubled;

        return Task.CompletedTask;
    }
}