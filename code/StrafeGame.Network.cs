
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
		Game.AssertServer();

		if ( !Game.IsServerHost ) return;

		var pkg = await Package.Fetch( Game.Server.MapIdent, true );
		var mapTitle = pkg?.Title ?? string.Empty;

		var msg = new ServerLogin()
		{
			SteamId = Game.ServerSteamId,
			ServerName = string.Empty, // no way to grab this yet?
			MapIdent = Game.Server.MapIdent,
			MapTitle = mapTitle,
			CourseType = CourseType
		};

		await Backend.Post<bool>( "server/login", msg.Serialize() );
	}

	private async void NetworkClientLogin( IClient client )
	{
		Game.AssertServer();

		if ( !Game.IsServerHost ) return;

		var msg = new ClientLogin()
		{
			ServerSteamId = (long)Game.ServerSteamId,
			Name = client.Name,
			PlayerId = client.SteamId,
			MapIdent = Game.Server.MapIdent
		};

		await Backend.Post<bool>( "player/login", msg.Serialize() );
	}

	[Event.Tick.Server]
	private void OnTick() => Connected = Backend.Connected;
	[Event.Entity.PostSpawn]
	private void SetMapState() => MapState = All.OfType<StrafeMapConfig>().FirstOrDefault()?.State ?? MapStates.Released;

}
