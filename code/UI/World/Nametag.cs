
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Strafe.Players;

namespace Strafe.UI;

internal class Nametag : WorldPanel
{

	private StrafePlayer Player;
	private Label Name;

	public Nametag( StrafePlayer player )
	{
		Player = player;

		StyleSheet.Load( "UI/Styles/_styles.scss" );
		Name = Add.Label( string.Empty, "name" );
		PanelBounds = new Rect( -1000, -1000, 2000, 2000 );
		SetClass( "local", player.IsLocalPawn );
	}

	[Event.Client.Frame]
	private void OnFrame()
	{
		if ( !Player.IsValid() ) return;
		if ( !Player.Client.IsValid() ) return;

		SetClass( "open", Target == Player );

		var hat = Player.GetAttachment( "hat" ) ?? new Transform( Player.EyePosition );
		Position = hat.Position + Vector3.Up * 8;
		Rotation = Rotation.LookAt( -Screen.GetDirection( new Vector2( Screen.Width * 0.5f, Screen.Height * 0.5f ) ) );

		if ( string.IsNullOrWhiteSpace( Name.Text ) )
		{
			Name.Text = Player.Client.Name;
		}
	}

	private static Entity Target;
	[Event.Tick.Client]
	public static void LookingAtAnybody()
	{
		if ( Camera.Current == null ) return;
		var from = Camera.Current.Position;
		var to = from + Camera.Current.Rotation.Forward * 600f;
		var tr = Trace.Ray( from, to )
			.WithTag( "player" )
			.Ignore( Local.Pawn )
			.Run();

		Target = tr.Entity;
	}

}
