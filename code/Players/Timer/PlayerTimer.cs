
using Sandbox;

namespace Strafe.Players;

internal partial class PlayerTimer : BaseNetworkable
{

	[Net]
	public StrafePlayer Player { get; set; }
	[Net, Predicted]
	public TimerStates State { get; set; }
	[Net, Predicted]
	public RealTimeSince Time { get; set; }

	public void Restart()
	{
		State = TimerStates.Start;
		Time = 0;

		Game.Current?.MoveToSpawnpoint( Player );
		Player.Velocity = 0;
		Player.BaseVelocity = 0;
		//Player.ResetInterpolation(); // this is predicted so no need
	}

}

public enum TimerStates
{
	Start,
	Live,
	Finished,
	Cancelled
}
