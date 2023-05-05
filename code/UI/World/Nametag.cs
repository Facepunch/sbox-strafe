
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

	[GameEvent.Client.Frame]
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
	[GameEvent.Tick.Client]
	public static void LookingAtAnybody()
	{
		if ( Game.LocalPawn is not StrafePlayer pl )
		{
			Target = null;
			return;
		}

		var from = pl.EyePosition;
		var to = from + pl.EyeRotation.Forward * 600f;
		var tr = Trace.Ray( from, to )
			.WithTag( "player" )
			.Ignore( Game.LocalPawn )
			.Run();

		Target = tr.Entity;
	}

}
