

using Sandbox;
using Strafe.Utility;

namespace Strafe.Api.Messages;

internal class ServerHeartbeat
{

	public int PlayerCount { get; set; }
	public string Map { get; set; }

	private static TimeSince TimeSincePing = 0;
	[Event.Tick.Server]
	public static async void Test()
	{
		if( TimeSincePing > 10 )
		{
			var ping = new ServerHeartbeat()
			{
				PlayerCount = Client.All.Count
			};
			await Backend.Post( "server/heartbeat", ping.Serialize() );
			TimeSincePing = 0;
		}
	}

}
