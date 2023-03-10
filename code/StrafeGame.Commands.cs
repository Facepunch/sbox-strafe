
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
using Sandbox.Diagnostics;
using static Strafe.Players.HandheldViewModel;

namespace Strafe;

internal partial class StrafeGame
{

	public static async void ExecuteChatCommand( IClient cl, string command )
	{
		Assert.True( cl.IsValid() );

		if ( string.IsNullOrWhiteSpace( command ) ) return;
		if ( command[0] != '!' ) return;

		var args = command.Remove( 0, 1 ).Split( ' ' );
		var cmdName = args[0].ToLower();

		if ( cmdName == "r" && Game.IsClient )
		{
			(Game.LocalPawn as StrafePlayer).ButtonToSet = InputButton.Drop;
		}

		if ( cmdName == "t" && Game.IsClient )
		{
			(Game.LocalPawn as StrafePlayer).ButtonToSet = InputButton.Reload;
		}

		if ( cmdName == "ping" && Game.IsServer )
		{
			var result = await Backend.Get<string>( "ping" );
			Chatbox.AddChatEntry( To.Everyone, "Response", result );
		}

		if ( cmdName == "wt" && Game.IsServer )
		{
			var result = await Backend.Post<string>( "ping/whitelisted", "someData" );
			Chatbox.AddChatEntry( To.Everyone, "Response", result );
		}

		if( cmdName == "noclip" && Game.IsServer )
		{
			Current.DoPlayerNoclip( cl );
		}

		if ( cmdName == "testmenu" && Game.IsServer )
		{
			if ( cl.Pawn is not StrafePlayer pl ) return;

			var menu = new SlotMenu();
			menu.Title = "Test Menu";
			menu.AddOption( "Option 1", x => Log.Error( "Option 1" ) );
			menu.AddOption( "Option 2", x => Log.Error( "Option 2" ) );
			menu.AddOption( "Option 3", x => Log.Error( "Option 3" ) );
		}

		if ( cmdName == "timeleft" && Game.IsServer )
		{
			Current.PrintTimeLeft();
		}

		if ( cmdName == "rtv" && Game.IsServer )
		{
			Current.RockTheVote( cl );
		}

		if ( cmdName == "nextmap" && Game.IsServer )
		{
			Chatbox.AddChatEntry( To.Everyone, "Server", $"The next map is {Current.NextMap ?? "undecided"}" );
		}

		if ( cmdName == "snailtrail" && Game.IsClient )
		{
			if ( cl.Pawn is not StrafePlayer pl ) return;

			var enabled = pl.ToggleTrail();
			if ( enabled ) Chatbox.AddChatEntry( "Server", "Trail Enabled" );
			else Chatbox.AddChatEntry( "Server", "Trail Disabled" );
		}
		if ( cmdName == "toggleviewmodel" && Game.IsClient )
		{
			if ( cl.Pawn is not StrafePlayer pl ) return;
			
			if ( pl.Handheld is RocketLauncher rl )
			{
				rl.ViewModel.viewstateindex++;
				if ( rl.ViewModel.viewstateindex > 3 ) rl.ViewModel.viewstateindex = 0;
				rl.ViewModel.UpdateCameraPos();
				Chatbox.AddChatEntry( "Server", $"Centered Viewmodel {rl.ViewModel.viewstate}" );

			}
		}
		if ( cmdName == "showkeys" && Game.IsClient )
		{
			if ( cl.Pawn is not StrafePlayer pl ) return;

			pl.DisplayInput = !pl.DisplayInput;
			
			if ( pl.DisplayInput ) Chatbox.AddChatEntry( "Server", "Input Display Enabled" );
			else Chatbox.AddChatEntry( "Server", "Input Display Disabled" );
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
			Chatbox.AddChatEntry( To.Single( caller ), "Timer", $"A replay was requested recently, try again in a few seconds.", "timer" );
			return;
		}

		if ( All.Any( x => x is ReplayEntity rep && rep.PlayerId == caller.SteamId ) )
		{
			Chatbox.AddChatEntry( To.Single( caller ), "Timer", $"There's already a replay spawned for this player.", "timer" );
			return;
		}

		TimeSinceReplaySpawned = 0f;

		Chatbox.AddChatEntry( To.Single( caller ), "Timer", $"Fetching your replay...", "timer" );

		var pb = await Backend.FetchPersonalBest( Game.Server.MapIdent, 0, caller.SteamId );

		if ( pb == null )
		{
			Chatbox.AddChatEntry( To.Single( caller ), "Timer", $"You haven't completed this map yet.", "timer" );
			return;
		}

		var replay = await Backend.FetchReplay( Game.Server.MapIdent, 0, pb.Rank );

		if ( replay == null )
		{
			Chatbox.AddChatEntry( To.Single( caller ), "Timer", $"You don't have a replay on this map.", "timer" );
			return;
		}

		Chatbox.AddChatEntry( To.Single( caller ), "Timer", $"Your replay has been spawned, it will play 4 times.", "timer" );

		ReplayEntity.Play( replay, 4, TimerStyles.Normal );
	}

}
