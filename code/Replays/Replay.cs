
using Strafe.Players;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Strafe.Replays;

internal class Replay
{

	private Replay() { }

	public DateTimeOffset Created { get; private set; }
	public long PlayerId { get; private set; }
	public IReadOnlyList<TimerFrame> Frames { get; private set; }

	public static Replay Create( IEnumerable<TimerFrame> frames, long playerid )
	{
		return new Replay()
		{
			Frames = frames.ToArray(),
			PlayerId = playerid,
			Created = DateTimeOffset.UtcNow
		};
	}

}
