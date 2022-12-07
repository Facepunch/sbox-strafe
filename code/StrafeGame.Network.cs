
using Sandbox;
using Strafe.Api;
using Strafe.Api.Messages;
using Strafe.Map;
using Strafe.Utility;
using System.Linq;

namespace Strafe;

internal partial class StrafeGame
{

	[Net]
	public bool Connected { get; set; }
	[Net]
	public MapStates MapState { get; set; }

	private async void NetworkServerLogin()
	{
		Host.AssertServer();

		if ( !Global.IsServerHost ) return;

		var pkg = await Package.Fetch( Global.MapName, true );
		var mapTitle = pkg?.Title ?? string.Empty;

		var msg = new ServerLogin()
		{
			SteamId = Global.ServerSteamId,
			ServerName = string.Empty, // no way to grab this yet?
			MapIdent = Global.MapName,
			MapTitle = mapTitle,
			CourseType = CourseType
		};

		await Backend.Post<bool>( "server/login", msg.Serialize() );
	}

	private async void NetworkClientLogin( Client client )
	{
		Host.AssertServer();

		if ( !Global.IsServerHost ) return;

		var msg = new ClientLogin()
		{
			ServerSteamId = (long)Global.ServerSteamId,
			Name = client.Name,
			PlayerId = client.SteamId,
			MapIdent = Global.MapName
		};

		await Backend.Post<bool>( "player/login", msg.Serialize() );
	}

	[Event.Tick.Server]
	private void OnTick() => Connected = Backend.Connected;
	[Event.Entity.PostSpawn]
	private void SetMapState() => MapState = All.OfType<StrafeMapConfig>().FirstOrDefault()?.State ?? MapStates.Released;

}
