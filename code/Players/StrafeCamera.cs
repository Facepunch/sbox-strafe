
using Sandbox;

namespace Strafe.Players;

internal class StrafeCamera : CameraMode
{

	public override void Update()
	{
		if ( Local.Pawn is not StrafePlayer pl ) 
			return;

		var target = Local.Pawn;

		if ( pl.SpectateTarget.IsValid() )
		{
			target = pl.SpectateTarget;
		}

		Position = target.EyePosition;
		Rotation = target.EyeRotation;

		Viewer = target;
	}

}
