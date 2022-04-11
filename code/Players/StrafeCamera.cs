
using Sandbox;

namespace Strafe.Players;

internal class StrafeCamera : CameraMode
{

	public override void Update()
	{
		var pawn = Local.Pawn;
		if ( pawn == null ) return;

		Position = pawn.EyePosition;
		Rotation = pawn.EyeRotation;

		Viewer = pawn;
	}

}
