
using Strafe.Players;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Strafe.Api.Messages;

internal class CompletionData 
{

	public string MapIdent { get; set; }
	public long PlayerId { get; set; }
	public int Stage { get; set; }
	public Dictionary<int, TimerFrame> Stats { get; set; } = new();
	public DateTimeOffset Date { get; set; }
	public string ReplayUrl { get; set; }

	public static CompletionData From( Strafe.Players.TimerEntity timer )
	{
		if ( timer.Owner is not StrafePlayer pl ) return null;

		var result = new CompletionData();
		result.PlayerId = pl.Client.SteamId;
		result.Stage = timer.Stage;
		result.MapIdent = Global.MapName;
		result.Date = DateTimeOffset.UtcNow;

		if ( timer.Stage == 0 )
		{
			var allTimers = timer.Owner?.Children?.OfType<Strafe.Players.TimerEntity>() ?? new List<Strafe.Players.TimerEntity>();

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
