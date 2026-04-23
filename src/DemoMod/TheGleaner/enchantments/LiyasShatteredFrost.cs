using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Enchantments;

/// <summary>
/// 艾黎的碎霜：抽1张牌，消耗。
/// </summary>
public sealed class LiyasShatteredFrost : EnchantmentModel
{
    public override bool HasExtraCardText => true;

    protected override void OnEnchant()
    {
        if (Card == null)
            return;

        CardCmd.ApplyKeyword(Card, CardKeyword.Exhaust);
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        if (Card?.Owner == null)
            return;

        await CardPileCmd.Draw(choiceContext, 1, Card.Owner, false);
    }
}