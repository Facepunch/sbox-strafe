
using Sandbox;
using System.Linq;

namespace Strafe;

public struct MoveHelper
{
	public Vector3 Position;
	public Vector3 Velocity;
	public bool HitWall;


	public float GroundBounce;
	public float WallBounce;
	public float MaxStandableAngle;
	public Trace Trace;

	public Vector3 Mins;
	public Vector3 Maxs;

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

	private bool TraceIsShit( TraceResult tr )
	{
		if ( (tr.Hit || tr.StartedSolid) && tr.Normal == default )
			return true;

		// in buggy ramp debug this is one of our fucked hits
		//if( tr.Normal == new Vector3( 0.3944f, 0.7537f, 0.5258f ) )
		//{
		//	Log.Info( tr.Normal.Dot( Velocity.Normal ) );
		//}

		if ( tr.Normal.Dot( Velocity.Normal ) < -.3f )
			return true;

		return false;
	}

	private TraceResult FudgeTrace( TraceResult pm, float timeLeft )
	{
		var original = pm;

		for( int attempts = 0; attempts < 100; attempts++ )
		{
			var lift = attempts * .01f;
			var altpos = Position + Vector3.Random.Normal * lift;

			Trace = Trace.Size( Mins.WithZ( lift ), Maxs );

			pm = TraceFromTo( altpos, altpos + (Velocity * timeLeft) + (Vector3.Down * lift) );

			if ( !TraceIsShit( pm ) ) break;
		}

		if ( TraceIsShit( pm ) )
		{
			pm = original;
		}

		if( BasePlayerController.Debug )
		{
			if ( TraceIsShit( pm ) )
			{
				Log.Error( "Couldn't fudge trace to something acceptable" );
			}
			else
			{
				DebugOverlay.Text( original.EndPosition, "Dodged shit trace", Color.Yellow, 30f, 2000 );
				DebugOverlay.Line( original.EndPosition, original.EndPosition + original.Normal * 100f, Color.Red, 30f );
			}
		}

		Trace = Trace.Size( Mins, Maxs );

		return pm;
	}

	public float TryMove( float timestep )
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

			if ( TraceIsShit( pm ) )
			{
				var altpm = FudgeTrace( pm, timeLeft );
				pm.StartedSolid = altpm.StartedSolid;
				pm.Normal = altpm.Normal;
			}

			Trace = Trace.Size( Mins, Maxs );

			travelFraction += pm.Fraction;

			if ( pm.Fraction > 0.03125f )
			{
				// We want to move out from the end pos by a tiny margin,
				// sometimes sweeps will consider this end pos as starting in solid, which we have to get unstuck from
				Position = pm.EndPosition + pm.Normal * .001f;

				if ( pm.Fraction == 1 )
					break;

				moveplanes.StartBump( Velocity );
			}

			if ( bump == 0 && pm.Hit && pm.Normal.Angle( Vector3.Up ) >= MaxStandableAngle )
			{
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
