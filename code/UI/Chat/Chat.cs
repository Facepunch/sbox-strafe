
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
		set
		{
			if ( value ) OnOpenChat();
			else Close();
		}
	}

	public Chat()
	{
		Current = this;

		Input.AddEventListener( "onsubmit", () => Submit( Input.Text ) );
		Input.AddEventListener( "onblur", Close );

		Canvas.PreferScrollToBottom = true;
	}

	private void OnOpenChat()
	{
		SetClass( "open", true );
		Input.Focus();
		Input.Text = string.Empty;
	}

	private void Close()
	{
		SetClass( "open", false );
		Input.Text = string.Empty;

		foreach ( var child in Children )
		{
			Unselect( child );
		}
	}

	public static void Submit( string msg )
	{
		if ( string.IsNullOrWhiteSpace( msg ) )
			return;

		Say( msg );
	}

	public void AddEntry( string name, string message, string classes = default )
	{
		var msg = Canvas.AddChild<StrafeChatEntry>();
		msg.Name = name;
		msg.Message = message;
		msg.AddClass( classes );

		Canvas.TryScrollToBottom();

		Sound.FromScreen( "ui.button.over" );
	}

	[Event.BuildInput]
	private void OnBuildInput( InputBuilder b )
	{
		if ( b.Pressed( InputButton.Chat ) )
		{
			Open = !Open;
		}
	}

	[ConCmd.Client( "chat_add_entry", CanBeCalledFromServer = true )]
	public static void AddChatEntry( string name, string message, string classes = default )
	{
		Current?.AddEntry( name, message, classes );

		// Only log clientside if we're not the listen server host
		if ( !Global.IsListenServer )
		{
			Log.Info( $"{name}: {message}" );
		}
	}

	protected override void OnMouseDown( MousePanelEvent e )
	{
		base.OnMouseDown( e );

		foreach ( var child in Children )
		{
			Unselect( child );
		}
	}

	private void Unselect( Panel p )
	{
		if ( p is Label l )
		{
			l.ShouldDrawSelection = false;
			return;
		}

		foreach ( var child in p.Children )
		{
			Unselect( child );
		}
	}

	[ConCmd.Client]
	public static void Say( string message )
	{
		if ( message[0] == '!' )
		{
			StrafeGame.ExecuteChatCommand( Local.Client, message );
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
