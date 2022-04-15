
using Sandbox;
using Sandbox.UI;
using Strafe.Players;
using Strafe.Utility;

namespace Strafe.UI;

[Hud, UseTemplate]
internal class TimerHud : Panel
{

	public int Speedometer => (int)(Local.Pawn?.Velocity.WithZ( 0 ).Length ?? 0);
	public string Timer
	{
		get
		{
			if ( Local.Pawn is not StrafePlayer pl ) 
				return "Disabled";

			if ( pl.Timer.State == TimerStates.Live )
				return pl.Timer.Time.ToTimeMs();

			return "Idle";
		}
	}

}
