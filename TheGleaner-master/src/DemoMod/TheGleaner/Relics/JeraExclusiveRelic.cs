using System.Linq;
using BaseLib.Abstracts;
using DemoMod.TheGleaner.Characters;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace DemoMod.TheGleaner.Relics;

public abstract class JeraExclusiveRelic : CustomRelicModel
{
	public override bool IsAllowed(IRunState runState)
	{
		if (((runState != null) ? ((IPlayerCollection)runState).Players : null) == null)
		{
			return false;
		}
		return ((IPlayerCollection)runState).Players.Any(IsJeraPlayer);
	}

	protected bool IsOwnerJera()
	{
		return IsJeraPlayer(((RelicModel)this).Owner);
	}

	protected static bool IsJeraPlayer(Player? player)
	{
		if (((player != null) ? player.Character : null) == null)
		{
			return false;
		}
		if (player.Character is DemoMod.TheGleaner.Characters.TheGleaner)
		{
			return true;
		}
		return ((AbstractModel)player.Character).Id == ((AbstractModel)ModelDb.Character<DemoMod.TheGleaner.Characters.TheGleaner>()).Id;
	}

	protected JeraExclusiveRelic()
		: base(true)
	{
	}
}
