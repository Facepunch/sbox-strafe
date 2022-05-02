
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

			var stage = pl.Stage( 0 );
			if ( stage.State != TimerEntity.States.Live )
				return stage.State.ToString();

			return stage.Timer.HumanReadable();
		}
	}

	public string Where
	{
		get
		{
			if ( StrafeGame.Current.CourseType == CourseTypes.Linear )
			{
				return $"CP {Checkpoint}";
			}

			if ( StrafeGame.Current.CourseType == CourseTypes.Staged )
			{
				return $"Stage {Stage}";
			}

			return "Map is invalid";
		}
	}

	public string Stats
	{
		get
		{
			return $"{Jumps} jumps\n{Strafes} strafes";
		}
	}

	public int Stage => (Local.Pawn as StrafePlayer)?.CurrentStage().Stage ?? 0;
	public int Checkpoint => (Local.Pawn as StrafePlayer)?.CurrentStage().Stage ?? 0;
	public int Jumps => (Local.Pawn as StrafePlayer)?.Stage( 0 ).Jumps ?? 0;
	public int Strafes => (Local.Pawn as StrafePlayer)?.Stage( 0 ).Strafes ?? 0;

}
