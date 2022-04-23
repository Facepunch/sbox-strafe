
using Sandbox;
using System.Linq;

namespace Strafe;

public struct MoveHelper
{
	public Vector3 Position;
	public Vector3 Velocity;
	public bool HitWall;
	public Vector3 WallNormal;


	public float GroundBounce;
	public float WallBounce;
	public float MaxStandableAngle;
	public Trace Trace;

	public MoveHelper( Vector3 position, Vector3 velocity ) : this()
	{
		Velocity = velocity;
		Position = position;
		GroundBounce = 0.0f;
		WallBounce = 0.0f;
		MaxStandableAngle = 10.0f;

		Trace = Trace.Ray( 0, 0 )
						.WorldAndEntities()
						.HitLayer( CollisionLayer.All, false )
						.HitLayer( CollisionLayer.Solid, true )
						.HitLayer( CollisionLayer.GRATE, true )
						.HitLayer( CollisionLayer.PLAYER_CLIP, true )
						.HitLayer( CollisionLayer.WINDOW, true )
						.HitLayer( CollisionLayer.NPC, true );
	}

	public TraceResult TraceFromTo( Vector3 start, Vector3 end )
	{
		return Trace.FromTo( start, end ).Run();
	}

	public TraceResult TraceDirection( Vector3 down )
	{
		return TraceFromTo( Position, Position + down );
	}

	public float TryMove( float timestep, Vector3 prevnormal = default )
	{
		var timeLeft = timestep;
		float travelFraction = 0;
		HitWall = false;

		using var moveplanes = new VelocityClipPlanes( Velocity, 5 );

		var lastnormal = Vector3.Zero;

		for ( int bump = 0; bump < moveplanes.Max; bump++ )
		{
			if ( Velocity.Length.AlmostEqual( 0.0f ) )
				break;

			var pm = TraceFromTo( Position, Position + Velocity * timeLeft );

			//DebugOverlay.Line( Position, Position + pm.Normal * 100f, 10f );

			travelFraction += pm.Fraction;

			if ( pm.Fraction > 0.03125f )
			{
				// We want to move out from the end pos by a tiny margin,
				// sometimes sweeps will consider this end pos as starting in solid, which we have to get unstuck from
				Position = pm.EndPosition + pm.Normal * 0.01f;

				if ( pm.Fraction == 1 )
					break;

				moveplanes.StartBump( Velocity );
			}

			if ( bump == 0 && pm.Hit && pm.Normal.Angle( Vector3.Up ) >= MaxStandableAngle )
			{
				WallNormal = pm.Normal;
				HitWall = true;
			}

			timeLeft -= timeLeft * pm.Fraction;

			if ( !moveplanes.TryAdd( pm.Normal, ref Velocity, IsFloor( pm ) ? GroundBounce : WallBounce ) )
				break;
		}

		if ( travelFraction == 0 )
			Velocity = 0;

		return travelFraction;
	}

	public bool IsFloor( TraceResult tr )
	{
		if ( !tr.Hit ) return false;
		return tr.Normal.Angle( Vector3.Up ) < MaxStandableAngle;
	}

}
