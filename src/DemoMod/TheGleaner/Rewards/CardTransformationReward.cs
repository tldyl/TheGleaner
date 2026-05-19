using DemoMod.TheGleaner.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace DemoMod.TheGleaner.Rewards;
public class CardTransformationReward(Player player) : Reward(player) {
    protected override RewardType RewardType => RewardType.RemoveCard;
    public override int RewardsSetIndex => 7;
    protected override string IconPath => ImageHelper.GetImagePath("ui/reward_screen/reward_icon_card_removal.png");
    public override LocString Description => new LocString("cards", "DEMOMOD-SIGHT_REAPING.rewardDescription");
    public override bool IsPopulated => true;
    
    public override Task Populate() => Task.CompletedTask;

    protected override async Task<bool> OnSelect() {
        await RunManager.Instance.RewardSynchronizer.DoLocalCardTransform(1);
        return true;
    }

    public override void MarkContentAsSeen() {

    }
}
