
namespace Strafe;

using Strafe.GameConfigs;
using System.Linq;

internal partial class StrafeGame : GameManager
{

	public static new StrafeGame Current;

	public BaseGameConfig GameConfig { get; private set; }

	public StrafeGame()
	{
		Current = this;

		if ( Game.IsServer )
		{
			Game.TickRate = 100;

			_ = new RunSubmitter();
			_ = new CprEntity();
			_ = GameLoopAsync();
			_ = DownloadWrReplay();

			All.OfType<SpawnPoint>().ToList().ForEach( x => x.Transmit = TransmitType.Always );
		}

		if ( Game.IsClient )
		{
			// todo: there's a bug where this seems to be calling again on client?
			// it's kinda rare and I don't know how to repro yet.
			Game.RootPanel?.Delete( true );
			Game.RootPanel = new Hud();

			Log.Info( "------------------------------------------" );
			Log.Info( "Welcome to Strafe, here's some copyable links" );
			Log.Info( "Website: https://strafedb.com" );
			Log.Info( "Discord: https://discord.gg/UG2KQdrkA5" );
			Log.Info( "------------------------------------------" );
		}
	}

	public override void ClientJoined( IClient cl )
	{
		base.ClientJoined( cl );

		cl.Pawn = new StrafePlayer();
		//(cl.Pawn as StrafePlayer).LoadClothing( cl );
		cl.Voice.WantsStereo = false;

		if ( cl.IsBot )
		{
			//(cl.Pawn as StrafePlayer).LoadClothing( Game.Clients.First() );
		}

		NetworkClientLogin( cl );

		Chatbox.AddChatEntry( To.Everyone, "Server", $"{cl.Name} has joined the game", "connect" );
		Chatbox.AddChatEntry( To.Single( cl ), "Server", "Website: https://strafedb.com", "important" );
		Chatbox.AddChatEntry( To.Single( cl ), "Server", "Discord: https://discord.gg/UG2KQdrkA5", "important" );
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		Chatbox.AddChatEntry( To.Everyone, "Server", $"{cl.Name} has disconnected", "connect" );
	}

	public override void MoveToSpawnpoint( Entity pawn )
	{
		base.MoveToSpawnpoint( pawn );

		pawn.Position = pawn.Position.WithZ( (int)(pawn.Position.z + 1) );
	}

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();

		var mapConfig = Entity.All.OfType<StrafeMapConfig>().FirstOrDefault();
		if ( mapConfig == null ) return;

		switch ( mapConfig.Type ) 
		{
			case MapTypes.RocketJump:
				GameConfig = new RocketJumpConfig();
				break;
			case MapTypes.Quake:
				GameConfig = new Quake3Config();
				break;
		}
	}

	public void DoPlayerNoclip( IClient player )
	{
		if ( player.Pawn is not StrafePlayer pl ) 
			return;
		if ( pl.Controller is not StrafeController ctrl ) 
			return;

		ctrl.Noclip = !ctrl.Noclip;

		if ( !Game.IsServer ) return;

		if ( ctrl.Noclip )
		{
			Chatbox.AddChatEntry( To.Single( player ), "Server", "Noclip enabled", "server" );
		}
		else
		{
			Chatbox.AddChatEntry( To.Single( player ), "Server", "Noclip disabled", "server" );
		}
	}

	public override void OnVoicePlayed( IClient cl )
	{
		VoiceChatList.Current?.OnVoicePlayed( cl.SteamId, cl.Voice.CurrentLevel );
	}

	[ConCmd.Server]
	public static void AddBot()
	{
		new Bot();
	}

}
