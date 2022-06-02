
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
			(Local.Pawn as StrafePlayer).ButtonToSet = InputButton.Drop;
		}

		if ( cmdName == "t" && Host.IsClient )
		{
			(Local.Pawn as StrafePlayer).ButtonToSet = InputButton.Reload;
		}

		if ( cmdName == "ping" && Host.IsServer )
		{
			var result = await Backend.Get<string>( "ping" );
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

		if ( cmdName == "snailtrail" && Host.IsClient )
		{
			if ( cl.Pawn is not StrafePlayer pl ) return;

			Log.Info( Time.Now );
			
			var enabled = pl.ToggleTrail();
			if ( enabled ) Chat.AddChatEntry( "Server", "Trail Enabled" );
			else Chat.AddChatEntry( "Server", "Trail Disabled" );
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
