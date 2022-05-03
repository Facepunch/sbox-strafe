
using Sandbox;
using Strafe.Api;
using Strafe.Leaderboards;
using Strafe.Players;
using Strafe.UI;

namespace Strafe;

internal partial class StrafeGame : Game
{

	public static new StrafeGame Current;

	public StrafeGame()
	{
		Current = this;

		if ( IsServer )
		{
			Global.TickRate = 100;

			_ = new UIEntity();
			_ = new RunSubmitter();
		}
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		client.Pawn = new StrafePlayer();
		(client.Pawn as StrafePlayer).Respawn();
	}

	public static async void ExecuteChatCommand( Client cl, string command )
	{
		Assert.True( cl.IsValid() );

		if ( string.IsNullOrWhiteSpace( command ) ) return;
		if ( command[0] != '!' ) return;

		var args = command.Remove( 0, 1 ).Split( ' ' );
		var cmdName = args[0].ToLower();

		if( cmdName == "r" && Host.IsClient )
		{
			(Local.Pawn as StrafePlayer).ButtonToSet = InputButton.Reload; 
		}

		if( cmdName == "t" && Host.IsClient )
		{
			(Local.Pawn as StrafePlayer).ButtonToSet = InputButton.Drop;
		}

		if( cmdName == "ping" && Host.IsClient )
		{
			var result = await StrafeApi.Fetch<string>( "ping" );
			Chat.AddChatEntry( "Response", result ); 
		}
	}

	public override void MoveToSpawnpoint( Entity pawn )
	{
		base.MoveToSpawnpoint( pawn );

		var pos = pawn.Position;
		pos.z = (int)(pos.z + 1);
		pawn.Position = pos;
	}

}

