
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

		var result = await Backend.Post<ClientLoginResult>( "player/login", msg.Serialize() );

		if ( result.CreditsAwarded > 0 )
		{
			Chatbox.AddChatEntry( To.Single( client ), "Shop", $"Daily login reward: {result.CreditsAwarded} \U0001fa99", "store" );
		}

		if ( client.Pawn is StrafePlayer pl )
		{
			pl.Credits = result.TotalCredits;
		}
	}

	[GameEvent.Tick.Server]
	private void OnTick() => Connected = Backend.Connected;
	[GameEvent.Entity.PostSpawn]
	private void SetMapState() => MapState = All.OfType<StrafeMapConfig>().FirstOrDefault()?.State ?? MapStates.Released;

}
