
using Strafe.Players;
using System.Collections.Generic;

namespace Strafe.Leaderboards;

internal class Replay
{

	public Replay( List<TimerFrame> frames ) 
	{
		Frames = frames;
	}

	public readonly IReadOnlyList<TimerFrame> Frames;

}
