
using Sandbox;

namespace Strafe.Players;

class StrafeController : WalkController
{

	public override void AirMove()
	{
		SurfaceFriction = 1f;
		AirControl = 60f;

		base.AirMove();
	}

}

