
using Sandbox;

namespace Strafe.Players;

partial class StrafeController : WalkController
{

	public override void Simulate()
	{
		if ( Pawn is not StrafePlayer pl ) return;

		var prevz = BaseVelocity.z;
		var prevmomentum = pl.Momentum;

		ApplyMomentum();

		base.Simulate();

		// hack in maintaining z velocity for boosting on small slopes and shit
		if ( prevmomentum && BaseVelocity.z.AlmostEqual( 0f ) )
		{
			BaseVelocity = BaseVelocity.WithZ( prevz );
		}
	}

	public override void AirMove()
	{
		SurfaceFriction = 1f;

		base.AirMove();
	}

	private void ApplyMomentum()
	{
		if ( Pawn is not StrafePlayer pl ) return;

		if ( !pl.Momentum )
		{
			Velocity += (1.0f + (Time.Delta * 0.5f)) * BaseVelocity;
			BaseVelocity = Vector3.Zero;
		}

		pl.Momentum = false;
	}

}

