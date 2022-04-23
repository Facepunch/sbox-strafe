
using Sandbox;

namespace Strafe.Players;

internal partial class StrafePlayer
{

	[ConVar.Replicated]
	public static bool strafe_debug_buggyramp { get; set; }

	private static TimeSince TimeSinceLastDebug;

	[Event.Tick.Server]
	private void OnDebug()
	{
		if ( !strafe_debug_buggyramp ) return;
		if ( TimeSinceLastDebug < 1.75f ) return;

		Egypt_BugTest();
		TimeSinceLastDebug = 0;
	}

	[Event.BuildInput]
	private void OnDebugInput( InputBuilder b )
	{
		if ( !strafe_debug_buggyramp ) return;

		//b.ViewAngles = new( 9.26f, 167.92f, 0.00f );
		b.ViewAngles = new( 9.26f, 177.92f, 0.00f );
	}

	private void Egypt_BugTest()
	{
		if ( !IsServer ) return;

		Position = new( 1862.74f, 1143.19f, 221.57f );
		Rotation = Rotation.From( 9.26f, 177.92f, 0.00f );
		Velocity = Rotation.Forward * 1000f;
	}

}
