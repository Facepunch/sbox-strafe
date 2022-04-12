
using Sandbox;
using Strafe.Map;
using System.Collections.Generic;
using System.Linq;

namespace Strafe.Players;

partial class StrafeController : WalkController
{

	[Net, Predicted]
	public bool Momentum { get; set; }

	private List<StrafeTrigger> TouchingTriggers = new();
	private Vector3 prevBaseVelocity;

	public override void Simulate()
	{
		DoTriggers();

		prevBaseVelocity = BaseVelocity;

		ApplyMomentum();

		base.Simulate();
	}

	public override void AirMove()
	{
		SurfaceFriction = 1f;

		base.AirMove();
	}

	public override void CategorizePosition( bool bStayOnGround )
	{
		SurfaceFriction = 1.0f;

		// Doing this before we move may introduce a potential latency in water detection, but
		// doing it after can get us stuck on the bottom in water if the amount we move up
		// is less than the 1 pixel 'threshold' we're about to snap to.	Also, we'll call
		// this several times per frame, so we really need to avoid sticking to the bottom of
		// water on each call, and the converse case will correct itself if called twice.
		//CheckWater();

		var point = Position - Vector3.Up * 2;
		var vBumpOrigin = Position;

		//
		//  Shooting up really fast.  Definitely not on ground trimed until ladder shit
		//
		bool bMovingUpRapidly = Velocity.z + prevBaseVelocity.z > MaxNonJumpVelocity;
		bool bMovingUp = Velocity.z + prevBaseVelocity.z > 0;

		bool bMoveToEndPos = false;

		if ( GroundEntity != null ) // and not underwater
		{
			bMoveToEndPos = true;
			point.z -= StepSize;
		}
		else if ( bStayOnGround )
		{
			bMoveToEndPos = true;
			point.z -= StepSize;
		}

		if ( bMovingUpRapidly || Swimming ) // or ladder and moving up
		{
			ClearGroundEntity();
			return;
		}

		var pm = TraceBBox( vBumpOrigin, point, 4.0f );

		if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
		{
			ClearGroundEntity();
			bMoveToEndPos = false;

			if ( Velocity.z + prevBaseVelocity.z > 0 )
				SurfaceFriction = 0.25f;
		}
		else
		{
			UpdateGroundEntity( pm );
		}

		if ( bMoveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
		{
			Position = pm.EndPosition;
		}
	}

	private void ApplyMomentum()
	{
		if ( !Momentum )
		{
			Velocity += (1.0f + (Time.Delta * 0.5f)) * BaseVelocity;
			BaseVelocity = Vector3.Zero;
		}

		Momentum = false;
	}

	private void DoTriggers()
	{
		var triggers = Entity.All.OfType<StrafeTrigger>().Where( x => x.WorldSpaceBounds.Overlaps( Pawn.WorldSpaceBounds ) );
		var pl = Pawn as StrafePlayer;

		foreach ( var trigger in triggers )
		{
			if ( !TouchingTriggers.Contains( trigger ) )
			{
				trigger.SimulatedStartTouch( this );
			}
			else
			{
				trigger.SimulatedTouch( this );
			}
		}

		foreach ( var trigger in TouchingTriggers )
		{
			if ( !triggers.Contains( trigger ) )
			{
				trigger.SimulatedEndTouch( this );
			}
		}

		TouchingTriggers.Clear();
		TouchingTriggers.AddRange( triggers );
	}

}

