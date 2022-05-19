
using Sandbox.Internal;
using Strafe.Api.Messages;
using Strafe.Leaderboards;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Strafe.Api;

internal partial class Backend
{

	public static async Task<List<PersonalBestEntry>> FetchPersonalBests( string mapIdent, int stage, int amount, int skip )
	{
		return await Get<List<PersonalBestEntry>>( $"pb/fetch?map={mapIdent}&stage={stage}&amount={amount}&skip={skip}" );
	}

	public static async Task<CompletionData> FetchCompletion( string mapIdent, int stage, int rank )
	{
		return await Get<CompletionData>( $"completion/fetch?map={mapIdent}&stage={stage}&rank={rank}" );
	}

	public static async Task<Replay> FetchReplay( string mapIdent, int stage, int rank )
	{
		var completionData = await FetchCompletion( mapIdent, stage, rank );

		if ( completionData == null ) return null;
		if ( string.IsNullOrEmpty( completionData.ReplayUrl ) ) return null;

		try
		{
			var url = completionData.ReplayUrl;
			var client = new Http( new System.Uri( url ) );
			var data = await client.GetBytesAsync();
			return Replay.FromBytes( data );
		}
		catch(Exception e )
		{
			Log.Error( "Error fetching replay: " + e.Message );
			return null;
		}
	}

}
