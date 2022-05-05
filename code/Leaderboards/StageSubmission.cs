
using Strafe.Players;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Strafe.Leaderboards;

internal class StageSubmission
{

	public string MapIdent { get; set; }
	public long PlayerId { get; set; }
	public int Stage { get; set; }
	public Dictionary<int, TimerFrame> Stats { get; set; } = new();
	//public Replay Replay { get; set; }
	public DateTimeOffset Date { get; set; }

	public static StageSubmission From( TimerEntity timer )
	{
		if ( timer.Owner is not StrafePlayer pl ) return null;

		var result = new StageSubmission();
		result.PlayerId = pl.Client.PlayerId;
		result.Stage = timer.Stage;
		result.MapIdent = Global.MapName;
		result.Date = DateTimeOffset.UtcNow;

		if ( timer.Stage == 0 )
		{
			var allTimers = timer.Owner?.Children?.OfType<TimerEntity>() ?? new List<TimerEntity>();

			foreach ( var t in allTimers )
			{
				result.Stats[t.Stage] = t.Snapshot;
			}
		}
		else
		{
			result.Stats[timer.Stage] = timer.Snapshot;
		}

		return result;
	}

}
