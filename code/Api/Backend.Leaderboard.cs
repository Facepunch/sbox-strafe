
namespace Strafe.Api;

internal partial class Backend
{

	public static async Task<PersonalBestEntry> FetchPersonalBest( string mapIdent, int stage, long playerId, TimerStyles style = TimerStyles.Normal )
	{
		var response = await Get<List<PersonalBestEntry>>( $"pb/fetch?map={mapIdent}&stage={stage}&playerid={playerId}&style={(int)style}" );
		return response?.FirstOrDefault();
	}

	public static async Task<List<PersonalBestEntry>> FetchPersonalBests( string mapIdent, int stage, int amount, int skip, TimerStyles style = TimerStyles.Normal )
	{
		return await Get<List<PersonalBestEntry>>( $"pb/fetch?map={mapIdent}&stage={stage}&amount={amount}&skip={skip}&style={(int)style}" );
	}

	public static async Task<CompletionData> FetchCompletion( string mapIdent, int stage, int rank, TimerStyles style = TimerStyles.Normal )
	{
		return await Get<CompletionData>( $"completion/fetch?map={mapIdent}&stage={stage}&rank={rank}&style={(int)style}" );
	}

	public static async Task<Replay> FetchReplay( string mapIdent, int stage, int rank, TimerStyles style = TimerStyles.Normal )
	{
		var completionData = await FetchCompletion( mapIdent, stage, rank, style );

		if ( completionData == null ) return null;
		if ( string.IsNullOrEmpty( completionData.ReplayUrl ) ) return null;

		try
		{
			var url = completionData.ReplayUrl;
			var data = await Sandbox.Http.RequestBytesAsync( url );
			return Replay.FromBytes( data );
		}
		catch(Exception e )
		{
			Log.Error( "Error fetching replay: " + e.Message );
			return null;
		}
	}

}
