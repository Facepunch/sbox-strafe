
using Sandbox;
using Strafe.Leaderboards;
using Strafe.Players;
using Strafe.UI;
using System.Linq;

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

			_ = new RunSubmitter();
			_ = new CprEntity();
			_ = GameLoopAsync();
			_ = DownloadWrReplay();

			All.OfType<SpawnPoint>().ToList().ForEach( x => x.Transmit = TransmitType.Always );
		}

		if ( IsClient )
		{
			_ = new Hud();

			Log.Info( "------------------------------------------" );
			Log.Info( "Welcome to Strafe, here's some copyable links" );
			Log.Info( "Website: https://strafedb.com" );
			Log.Info( "Discord: https://discord.gg/UG2KQdrkA5" );
			Log.Info( "------------------------------------------" );
		}
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		cl.Pawn = new StrafePlayer();
		(cl.Pawn as StrafePlayer).Respawn();

		NetworkClientLogin( cl );

		Chat.AddChatEntry( To.Everyone, "Server", $"{cl.Name} has joined the game", "connect" );
		Chat.AddChatEntry( To.Single( cl ), "Server", "Website: https://strafedb.com", "important" );
		Chat.AddChatEntry( To.Single( cl ), "Server", "Discord: https://discord.gg/UG2KQdrkA5", "important" );
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		Chat.AddChatEntry( To.Everyone, "Server", $"{cl.Name} has disconnected", "connect" );
	}

	public override void MoveToSpawnpoint( Entity pawn )
	{
		base.MoveToSpawnpoint( pawn );

		pawn.Position = pawn.Position.WithZ( (int)(pawn.Position.z + 1) );
	}

	public override void DoPlayerNoclip( Client player )
	{
		if ( player.Pawn is not StrafePlayer pl ) 
			return;

		if( pl.DevController == null )
		{
			pl.DevController = new NoclipController();
			Chat.AddChatEntry( To.Single( player ), "Server", "Noclip enabled", "server" );
		}
		else
		{
			pl.DevController = null;
			Chat.AddChatEntry( To.Single( player ), "Server", "Noclip disabled", "server" );
		}
	}

}

