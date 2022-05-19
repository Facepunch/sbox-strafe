
using Sandbox;
using Strafe.Players;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Strafe.UI;

internal class PlayerList : EasyList<PlayerListEntry, StrafePlayer>
{

	protected override List<StrafePlayer> FetchItems()
	{
		return Entity.All.OfType<StrafePlayer>().ToList();
	}

	protected override int GetItemHash()
	{
		var result = -13;
		foreach ( var ent in Entity.All )
		{
			if ( ent is not StrafePlayer pl ) continue;
			result = HashCode.Combine( result, pl.NetworkIdent );
		}
		return result;
	}

}
