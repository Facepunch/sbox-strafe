
using Sandbox;

namespace Strafe.Players;

internal struct TimerFrame
{

	public Vector3 Velocity { get; set; }
	public Vector3 Position { get; set; }
	public Angles Angles { get; set; }
	public float Time { get; set; }
	public int Jumps { get; set; }
	public int Strafes { get; set; }

}
