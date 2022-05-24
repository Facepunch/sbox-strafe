
using Sandbox;
using Strafe.Api;
using Strafe.Players;
using Strafe.Menu;
using Strafe.UI;
using Sandbox.Internal;
using System.Collections.Generic;
using Strafe.Utility;
using System.IO;
using Strafe.Leaderboards;
using System.Linq;

namespace Strafe;

internal partial class StrafeGame
{

	public static async void ExecuteChatCommand( Client cl, string command )
	{
		Assert.True( cl.IsValid() );

		if ( string.IsNullOrWhiteSpace( command ) ) return;
		if ( command[0] != '!' ) return;

		var args = command.Remove( 0, 1 ).Split( ' ' );
		var cmdName = args[0].ToLower();

		if ( cmdName == "r" && Host.IsClient )
		{
			(Local.Pawn as StrafePlayer).ButtonToSet = InputButton.Reload;
		}

		if ( cmdName == "t" && Host.IsClient )
		{
			(Local.Pawn as StrafePlayer).ButtonToSet = InputButton.Drop;
		}

		if ( cmdName == "ping" && Host.IsServer )
		{
			var result = await Backend.Get<string>( "ping" );
			Chat.AddChatEntry( To.Everyone, "Response", result );
		}

		if ( cmdName == "blobtest" && Host.IsServer )
		{
			var result = await Backend.Post<string>( "blobtest/tester", new byte[4] { 1, 2, 3, 4 } );
			Chat.AddChatEntry( To.Everyone, "Response", result );
		}

		if ( cmdName == "wsping" && Host.IsServer )
		{
			var result = await Backend.Post<string>( "ping", "someData" );
			Chat.AddChatEntry( To.Everyone, "Response", result );
		}

		if ( cmdName == "wt" && Host.IsServer )
		{
			var result = await Backend.Post<string>( "ping/whitelisted", "someData" );
			Chat.AddChatEntry( To.Everyone, "Response", result );
		}

		if ( cmdName == "testmenu" && Host.IsServer )
		{
			if ( cl.Pawn is not StrafePlayer pl ) return;

			var menu = new SlotMenu();
			menu.Title = "Test Menu";
			menu.AddOption( "Option 1", x => Log.Error( "Option 1" ) );
			menu.AddOption( "Option 2", x => Log.Error( "Option 2" ) );
			menu.AddOption( "Option 3", x => Log.Error( "Option 3" ) );
		}

		if ( cmdName == "testvote" && Host.IsServer )
		{
			_ = Current.MapVoteAsync();
		}

		if ( cmdName == "timeleft" && Host.IsServer )
		{
			Current.PrintTimeLeft();
		}

		if ( cmdName == "rtv" && Host.IsServer )
		{
			Current.RockTheVote( cl );
		}

		if ( cmdName == "nextmap" && Host.IsServer )
		{
			Chat.AddChatEntry( To.Everyone, "Server", $"The next map is {Current.NextMap ?? "undecided"}" );
		}

		if ( cmdName == "snailtrail" && Host.IsServer )
		{
			if ( cl.Pawn is not StrafePlayer pl ) return;
			pl.EnableTrailParticle();

			if ( pl.TrailEnabled == true )
			{
				Chat.AddChatEntry( To.Single( cl ), "Server", "Trail Enabled" );
			}
			if ( pl.TrailEnabled == false )
			{
				Chat.AddChatEntry( To.Single( cl ), "Server", "Trail Disabled" );
			}
		}

		if ( cmdName == "dl" && Host.IsServer )
		{
			var http = new Http( new System.Uri( "https://strafereplays.blob.core.windows.net/replays/67efd50e-586e-475e-b9e7-815f0c97040d.bytes" ) );
			var data = await http.GetBytesAsync();
			Log.Info( string.Join( ',', data ) );
		}

		if ( cmdName == "repsize" && Host.IsServer )
		{
			var data = new List<TimerFrame>();
			for ( int i = 0; i < 180000; i++ )
			{
				data.Add( new()
				{
					Angles = Angles.Random,
					Jumps = Rand.Int( 0, 100 ),
					Strafes = Rand.Int( 0, 100 ),
					Position = Vector3.Random,
					Time = Rand.Float( 0, 100 ),
					Velocity = Vector3.Random
				} );
			}
			var replay = new Strafe.Leaderboards.Replay( 0, data );

			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();
			var bytes = replay.ToBytes();
			sw.Stop();

			Log.Info( "BYTE ARRAY MB: " + bytes.Length / 1024f / 1024f );
			Log.Info( "COMPRESSED MB: " + bytes.Compress().Length / 1024f / 1024f );
			Log.Info( "TIME: " + sw.ElapsedMilliseconds );
		}
	}

	private static TimeSince TimeSinceReplaySpawned;
	[ConCmd.Server]
	public static async void SpawnMyReplay()
	{
		if ( !ConsoleSystem.Caller.IsValid() ) return;

		var caller = ConsoleSystem.Caller;

		if ( TimeSinceReplaySpawned < 10 )
		{
			Chat.AddChatEntry( To.Single( caller ), "Timer", $"A replay was requested recently, try again in a few seconds.", "timer" );
			return;
		}

		if ( All.Any( x => x is ReplayEntity rep && rep.PlayerId == caller.PlayerId ) )
		{
			Chat.AddChatEntry( To.Single( caller ), "Timer", $"There's already a replay spawned for this player.", "timer" );
			return;
		}

		TimeSinceReplaySpawned = 0f;

		Chat.AddChatEntry( To.Single( caller ), "Timer", $"Fetching your replay...", "timer" );

		var pb = await Backend.FetchPersonalBest( Global.MapName, 0, caller.PlayerId );

		if ( pb == null )
		{
			Chat.AddChatEntry( To.Single( caller ), "Timer", $"You haven't completed this map yet.", "timer" );
			return;
		}

		var replay = await Backend.FetchReplay( Global.MapName, 0, pb.Rank );

		if ( replay == null )
		{
			Chat.AddChatEntry( To.Single( caller ), "Timer", $"You don't have a replay on this map.", "timer" );
			return;
		}

		Chat.AddChatEntry( To.Single( caller ), "Timer", $"Your replay has been spawned, it will play 4 times.", "timer" );

		ReplayEntity.Play( replay, 4 );
	}

}
