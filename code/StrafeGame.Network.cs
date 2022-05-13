
using Sandbox;
using Strafe.Api;
using Strafe.Utility;

namespace Strafe;

internal partial class StrafeGame
{

	[Net]
	public bool Connected { get; set; }

	private async void NetworkServerLogin()
	{
		Host.AssertServer();

		if ( !Global.IsDedicatedServer ) return;

		var pkg = await Package.Fetch( Global.MapName, true );
		var mapTitle = pkg?.Title ?? string.Empty;

		var msg = new ServerLoginMessage()
		{
			SteamId = Global.ServerSteamId,
			ServerName = string.Empty, // no way to grab this yet?
			MapIdent = Global.MapName,
			MapTitle = mapTitle,
			CourseType = CourseType
		};

		await StrafeApi.Post<bool>( "server/login", msg.Serialize() );
	}

	private async void NetworkClientLogin( Client client )
	{
		Host.AssertServer();

		if ( !Global.IsDedicatedServer ) return;

		var msg = new LoginMessage()
		{
			ServerSteamId = (long)Global.ServerSteamId,
			Name = client.Name,
			PlayerId = client.PlayerId,
			MapIdent = Global.MapName
		};

		await StrafeApi.Post<bool>( "player/login", msg.Serialize() );
	}

	[Event.Tick.Server]
	private void OnTick() => Connected = StrafeApi.Connected;

}
