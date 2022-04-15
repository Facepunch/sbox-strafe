
using Sandbox;
using Sandbox.UI;

namespace Strafe.UI;

[Hud, UseTemplate]
internal partial class Chat : Panel
{

	public static Chat Current;

	public Panel Canvas { get; set; }
	public TextEntry Input { get; set; }

	public bool Open
	{
		get => HasClass( "open" );
		set => SetClass( "open", value );
	}

	public Chat()
	{
		Current = this;

		Input.AddEventListener( "onsubmit", () => Submit( Input.Text ) );
		Input.AddEventListener( "onblur", Close );
		Sandbox.Hooks.Chat.OnOpenChat += OnOpenChat;

		Canvas.PreferScrollToBottom = true;
	}

	private void OnOpenChat()
	{
		Open = true;
		Input.Focus();
		Input.Text = string.Empty;
	}

	private void Close()
	{
		Input.Text = string.Empty;
		Open = false;
	}

	[ClientCmd( "say2" )]
	public static void Submit( string msg )
	{
		if ( string.IsNullOrWhiteSpace( msg ) )
			return;

		if ( msg[0] == '!' )
		{
			StrafeGame.ExecuteChatCommand( Local.Client, msg );
		}

		Say( msg );
	}

	public void AddEntry( string name, string message )
	{
		var msg = Canvas.AddChild<StrafeChatEntry>();
		msg.Name = name;
		msg.Message = message;

		Canvas.TryScrollToBottom();
	}

	[ClientCmd( "chat_add", CanBeCalledFromServer = true )]
	public static void AddChatEntry( string name, string message )
	{
		Current?.AddEntry( name, message );

		// Only log clientside if we're not the listen server host
		if ( !Global.IsListenServer )
		{
			Log.Info( $"{name}: {message}" );
		}
	}

	[ServerCmd( "say" )]
	public static void Say( string message )
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
