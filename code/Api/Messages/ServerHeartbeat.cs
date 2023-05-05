

using Sandbox;
using Strafe.Utility;

namespace Strafe.Api.Messages;

internal class ServerHeartbeat
{

	public int PlayerCount { get; set; }
	public string Map { get; set; }

	private static TimeSince TimeSincePing = 0;
	[GameEvent.Tick.Server]
	public static async void Test()
	{
		if( TimeSincePing > 10 )
		{
			TimeSincePing = 0;
			var ping = new ServerHeartbeat()
			{
				PlayerCount = Game.Clients.Count
			};
			await Backend.Post( "server/heartbeat", ping.Serialize() );
		}
	}

}
