using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Commands;
using DemoMod.TheGleaner.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace DemoMod.TheGleaner.Cards.GleanerCard;

[Pool(typeof(CardPool))]
public class Raiment : CustomCardModel {
	public override string PortraitPath => $"res://TheGleaner/images/cards/{Id.Entry.ToLowerInvariant()}.png";
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		.. HoverTipFactory.FromEnchantment<Swift>(2),
		.. HoverTipFactory.FromEnchantment<Sown>(),
		.. HoverTipFactory.FromEnchantment<Glam>(),
	];
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	public Raiment() : base(7, CardType.Skill, CardRarity.Rare, TargetType.Self) {
		
	}

	public override async Task BeforeCombatStart() {
		if (!IsInCombat || CombatState == null || !IsUpgraded || Owner.Deck.Cards.Contains(this)) {
			return;
		}

		await ScorePileCmd.AddCards(Owner.PlayerCombatState, Owner, this);
	}
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
		foreach (CardModel allCard in Owner.PlayerCombatState.AllCards) {
			List<EnchantmentModel> enchantments = [ModelDb.Enchantment<Swift>(), ModelDb.Enchantment<Sown>(), ModelDb.Enchantment<Glam>()];
			Owner.RunState.Rng.Shuffle.Shuffle(enchantments);
			foreach (EnchantmentModel enchantment in enchantments) {
				if (enchantment.CanEnchant(allCard)) {
					CardCmd.Enchant(enchantment.ToMutable(), allCard, enchantment is Swift ? 2 : 1);
					NCardEnchantVfx child = NCardEnchantVfx.Create(allCard);
					if (child != null) {
						NRun instance = NRun.Instance;
						if (instance != null)
							instance.GlobalUi.CardPreviewContainer.AddChildSafely(child);
					}
					break;
				}
			}
		}
	}
}
