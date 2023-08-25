
namespace Strafe.Api;

internal partial class Backend
{

	public static async Task<List<PlayerData>> FetchRankLeaderboard( int take, int skip )
	{
		return await Get<List<PlayerData>>( $"player/fetch/ranks?take={take}&skip={skip}" );
	}

	public static async Task<List<PlayerData>> FetchCreditsLeaderboard( int take, int skip )
	{
		return await Get<List<PlayerData>>( $"player/fetch/credits?take={take}&skip={skip}" );
	}

	public static async Task<string> FetchPlayerName( long playerId )
	{
		return await Get( $"player/name?steamid={playerId}" );
	}

}
