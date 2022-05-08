
using Sandbox;
using Strafe.Api;
using Strafe.Utility;

namespace Strafe;

internal partial class StrafeGame
{

	[Net]
	public bool Connected { get; set; }

	private async void NetworkMapBump()
	{
		Host.AssertServer();

		var pkg = await Package.Fetch( Global.MapName, true );
		var mapTitle = pkg?.Title ?? string.Empty;

		var msg = new MapBumpMessage()
		{
			MapIdent = Global.MapName,
			MapTitle = mapTitle,
			CourseType = CourseType,
			Host = Global.ServerSteamId.ToString()
		};

		await StrafeApi.Post<bool>( "map/bump", msg.Serialize() );
	}

	private async void NetworkLogin( Client client )
	{
		Host.AssertServer();

		var msg = new LoginMessage()
		{
			Host = Global.ServerSteamId.ToString(),
			Name = client.Name,
			PlayerId = client.PlayerId,
			MapIdent = Global.MapName
		};

		await StrafeApi.Post<bool>( "player/login", msg.Serialize() );
	}

	[Event.Tick.Server]
	private void OnTick() => Connected = StrafeApi.Connected;

}
