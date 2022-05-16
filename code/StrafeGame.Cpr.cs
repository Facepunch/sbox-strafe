using Sandbox;
using Strafe.Api;
using Strafe.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strafe;

internal partial class StrafeGame
{

	[Net]
	public Dictionary<int, TimerFrame> Cpr { get; set; } = new();

	private async Task<bool> UpdateCpr()
	{
		var wrcp = await Backend.FetchCompletion( Global.MapName, 0, 1 );
		if ( wrcp == null ) return false;

		Cpr = wrcp.Stats;

		return true;
	}

	public TimerFrame Diff( int stage, TimerFrame frame )
	{
		if ( !Cpr.ContainsKey( stage ) ) return default;

		return new TimerFrame()
		{
			Time = frame.Time - Cpr[stage].Time,
			Jumps = frame.Jumps - Cpr[stage].Jumps,
			Strafes = frame.Strafes - Cpr[stage].Strafes,
		};
	}

}
