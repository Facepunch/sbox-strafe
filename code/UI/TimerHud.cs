
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Strafe.Players;
using Strafe.Utility;

namespace Strafe.UI;

[Hud, UseTemplate]
internal class TimerHud : Panel
{

	public Label TimeDiff { get; set; }
	public Label SpeedDiff { get; set; }
	public Label JumpDiff { get; set; }
	public Label StrafeDiff { get; set; }

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

			return stage.Timer.ToTime();
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

	private void BuildDiff( TimerEntity timer )
	{
		var snapshot = StrafeGame.Current.CourseType == CourseTypes.Staged
			? timer.GrabFrame()
			: (Local.Pawn as StrafePlayer).Stage( 0 ).GrabFrame();

		var diff = StrafeGame.Current.Diff( timer.Stage, snapshot );
		if ( diff == default ) return;

		TimeDiff.Text = diff.Time.ToTime( true );
		TimeDiff.SetClass( "red", diff.Time < 0 );

		var diffSpeed = diff.Velocity.WithZ( 0 ).Length - snapshot.Velocity.WithZ( 0 ).Length;
		SpeedDiff.Text = $"{(int)diffSpeed} u/s";
		SpeedDiff.SetClass( "red", diffSpeed < 0 );
	}

	[Events.Timer.OnStageStop]
	public void OnStopped( TimerEntity timer )
	{
		if ( timer.Stage != 0 ) return;

		TimeDiff.Text = "";
		SpeedDiff.Text = "";
	}

	[Events.Timer.OnStageStart]
	public void OnStageStart( TimerEntity timer )
	{
		if ( timer.Owner is not StrafePlayer pl ) return;
		if ( !pl.IsLocalPawn ) return;
		if ( timer.Stage != 0 ) return;

		if ( StrafeGame.Current.Cpr?.ContainsKey( 1 ) ?? false )
		{
			var diff = pl.Velocity.Length - StrafeGame.Current.Cpr[1].Velocity.Length;
			SpeedDiff.Text = $"{(int)diff} u/s";
			SpeedDiff.SetClass( "red", diff < 0 );
		}
	}

	[Events.Timer.OnStageComplete]
	public void OnStage( TimerEntity timer )
	{
		if ( timer.Owner is not StrafePlayer pl ) return;
		if ( !pl.IsLocalPawn ) return;

		BuildDiff( timer );
	}

}
