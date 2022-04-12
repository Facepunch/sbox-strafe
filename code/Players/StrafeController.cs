
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

	public override void Simulate()
	{
		DoTriggers();

		var prevz = BaseVelocity.z;
		var prevmomentum = Momentum;

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

		foreach( var trigger in TouchingTriggers )
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

