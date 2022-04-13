
using Sandbox;
using Strafe.Players;
using Strafe.UI;
using System;
using System.Linq;

namespace Strafe;

internal partial class StrafeGame : Game
{

	public StrafeGame()
	{
		if ( IsServer )
		{
			Global.TickRate = 100;

			_ = new UIEntity();
		}
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		client.Pawn = new StrafePlayer();
		(client.Pawn as StrafePlayer).Respawn();
	}

	public static void ExecuteChatCommand( Client cl, string command )
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
	}

	public override void MoveToSpawnpoint( Entity pawn )
	{
		base.MoveToSpawnpoint( pawn );

		var pos = pawn.Position;
		pos.z = (int)(pos.z + 1);
		pawn.Position = pos;
	}

}

