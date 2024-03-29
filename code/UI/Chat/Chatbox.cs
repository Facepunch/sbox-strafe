﻿
using Sandbox.Diagnostics;

namespace Strafe.UI;

public partial class Chatbox
{

	static Chatbox Instance;

	public Chatbox() 
	{
		Instance = this;
	}

	[GameEvent.Client.BuildInput]
	void OnBuildInput()
	{
		if ( !Sandbox.Input.Pressed( "Chat" ) ) return;

		IsOpen = !IsOpen;

		if ( IsOpen )
		{
			Input.Focus();
		}

		StateHasChanged();
	}

	[ConCmd.Client( "chat_add_entry", CanBeCalledFromServer = true )]
	public static void AddChatEntry( string name, string message, string classes = default )
	{
		Instance?.AddEntry( name, message, classes );

		// Only log clientside if we're not the listen server host
		if ( !Game.IsListenServer )
		{
			Log.Info( $"{name}: {message}" );
		}
	}

	[ConCmd.Client]
	public static void Say( string message )
	{
		if ( message[0] == '!' )
		{
			StrafeGame.ExecuteChatCommand( Game.LocalClient, message );
		}

		Say2( message );
	}

	[ConCmd.Server]
	public static void Say2( string message )
	{
		Assert.NotNull( ConsoleSystem.Caller );

		// todo - reject more stuff
		if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
			return;

		if ( string.IsNullOrWhiteSpace( message ) )
			return;

		Log.Info( $"{ConsoleSystem.Caller}: {message}" );

		if ( message[0] == '!' )
		{
			StrafeGame.ExecuteChatCommand( ConsoleSystem.Caller, message );
			return;
		}

		AddChatEntry( To.Everyone, ConsoleSystem.Caller.Name, message );
	}

}
