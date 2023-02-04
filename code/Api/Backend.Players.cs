using Sandbox.Internal;
using System.Linq;

namespace Strafe.Api;

internal partial class Backend
{

	public static async Task<List<PlayerData>> FetchCreditsLeaderboard( int take, int skip )
	{
		return await Get<List<PlayerData>>( $"player/fetch/credits?take={take}&skip={skip}" );
	}

}
