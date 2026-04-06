using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DemoMod.TheGleaner.Powers;

public class TheLakeMirrorPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player)
        {
            return;
        }

        Flash();

        // 攻击 -> 敏捷（临时）
        if (cardPlay.Card.Type == CardType.Attack)
        {
            await PowerCmd.Apply<DemoTempDexterityPower>(
                Owner,
                Amount,
                Owner,
                null
            );
        }

        // 技能 -> 力量（临时）
        if (cardPlay.Card.Type == CardType.Skill)
        {
            await PowerCmd.Apply<DemoTempStrengthPower>(
                Owner,
                Amount,
                Owner,
                null
            );
        }
    }
}