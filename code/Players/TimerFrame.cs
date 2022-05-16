
using Sandbox;
using System;

namespace Strafe.Players;

internal struct TimerFrame
{

	public Vector3 Velocity { get; set; }
	public Vector3 Position { get; set; }
	public Angles Angles { get; set; }
	public float Time { get; set; }
	public int Jumps { get; set; }
	public int Strafes { get; set; }

	public static bool operator ==( TimerFrame a, TimerFrame b )
	{
		return a.GetHashCode() == b.GetHashCode();
	}

	public static bool operator !=( TimerFrame a, TimerFrame b )
	{
		return !(a.GetHashCode() == b.GetHashCode());
	}

	public override bool Equals( object obj )
	{
		return base.Equals( obj );
	}

	public override int GetHashCode()
	{
		return HashCode.Combine( Velocity, Position, Angles, Time, Jumps, Strafes );
	}

}
