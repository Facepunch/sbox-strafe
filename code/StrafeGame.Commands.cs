
using Sandbox;
using Strafe.Api;
using Strafe.Players;
using Strafe.Menu;
using Strafe.UI;

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
	}

}
