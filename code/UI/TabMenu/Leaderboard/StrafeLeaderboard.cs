
using Sandbox;
using Sandbox.UI;
using Strafe.Api;
using Strafe.Api.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Strafe.UI;

[UseTemplate]
internal class StrafeLeaderboard : EasyList<StrafeLeaderboardEntry, PersonalBestEntry>
{

	private TimeSince TimeSinceBuild;
	private int FakeHash;

	protected override async Task<List<PersonalBestEntry>> FetchItemsAsync()
	{
		var qq = await Backend.FetchPersonalBests( Global.MapName, 0, 10, 0 );

		return qq ?? new();
	}

	protected override int GetItemHash()
	{
		if( TimeSinceBuild > 60 )
		{
			FakeHash = Rand.Int( 9999 );
			TimeSinceBuild = 0;
		}
		return FakeHash;
	}

}
