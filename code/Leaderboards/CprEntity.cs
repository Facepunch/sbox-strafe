
using Sandbox;
using Strafe.Api;
using Strafe.Players;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Strafe.Leaderboards;

internal partial class CprEntity : Entity
{

	public static CprEntity Current;

	[Net]
	public IDictionary<int, TimerFrame> Cpr { get; set; }

	public CprEntity()
	{
		Current = this;

		Transmit = TransmitType.Always;
	}

	public override async void Spawn()
	{
		base.Spawn();

		_ = await UpdateCpr();
	}

	private async Task<bool> UpdateCpr()
	{
		var wrcp = await Backend.FetchCompletion( Game.Server.MapIdent, 0, 1 );
		if ( wrcp == null ) return false;

		Cpr = wrcp.Stats;

		return true;
	}

	public static bool TryGetDiff( int stage, TimerFrame frame, out TimerFrame result )
	{
		result = default;

		if ( (!Current?.Cpr?.ContainsKey( stage )) ?? false ) return false;

		result = new TimerFrame()
		{
			Time = frame.Time - Current.Cpr[stage].Time,
			Jumps = (frame.Jumps - Current.Cpr[stage].Jumps),
			Strafes = (frame.Strafes - Current.Cpr[stage].Strafes),
		};

		return true;
	}

}
