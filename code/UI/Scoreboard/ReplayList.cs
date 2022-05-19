
using Sandbox;
using Strafe.Leaderboards;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Strafe.UI;

internal class ReplayList : EasyList<ReplayListEntry, ReplayEntity>
{

	protected override List<ReplayEntity> FetchItems()
	{
		return Entity.All.OfType<ReplayEntity>().ToList();
	}

	protected override int GetItemHash()
	{
		var hash = -123;
		foreach( var ent in Entity.All )
		{
			if ( ent is not ReplayEntity ) continue;
			hash = HashCode.Combine( hash, ent.NetworkIdent );
		}
		return hash;
	}

}
