
using Sandbox;
using Sandbox.UI;
using Strafe.Leaderboards;
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

	public int Speed { get; set; }
	public string Time { get; set; }
	public int Stage { get; set; }
	public int Jumps { get; set; }
	public int Strafes { get; set; }

	[Event.Frame]
	private void OnFrame()
	{
		if ( Local.Pawn is not StrafePlayer pl )
		{
			Time = "Disabled";
			return;
		}

		TimerFrame frame;
		Strafe.Players.TimerEntity.States state;
		int stage;

		if( pl.SpectateTarget is ReplayEntity rep )
		{
			frame = rep.Frame;
			stage = 0;
			state = Strafe.Players.TimerEntity.States.Live;
		}
		else
		{
			var target = (pl.SpectateTarget as StrafePlayer) ?? pl;
			frame = target.TimerFrame;
			stage = target.TimerStage;
			state = target.TimerState;
		}

		Stage = stage;
		Speed = (int)frame.Velocity.WithZ( 0 ).Length;
		Jumps = frame.Jumps;
		Strafes = frame.Strafes;

		if( state != Strafe.Players.TimerEntity.States.Live )
		{
			Time = state.ToString();
		}
		else
		{
			Time = frame.Time.ToTime();
		}
	}

	public string Where
	{
		get
		{
			if ( StrafeGame.Current.CourseType == CourseTypes.Linear )
			{
				return $"CP {Stage}";
			}

			if ( StrafeGame.Current.CourseType == CourseTypes.Staged )
			{
				return $"Stage {Stage}";
			}

			return "Map is invalid";
		}
	}

	private void BuildDiff( Strafe.Players.TimerEntity timer )
	{
		var snapshot = StrafeGame.Current.CourseType == CourseTypes.Staged
			? timer.GrabFrame()
			: (Local.Pawn as StrafePlayer).Stage( 0 ).GrabFrame();

		if ( !CprEntity.TryGetDiff( timer.Stage, snapshot, out var diff ) ) return;

		TimeDiff.Text = diff.Time.ToTime( true );
		TimeDiff.SetClass( "red", diff.Time < 0 );

		var diffSpeed = diff.Velocity.WithZ( 0 ).Length - snapshot.Velocity.WithZ( 0 ).Length;
		SpeedDiff.Text = $"{(int)diffSpeed} u/s";
		SpeedDiff.SetClass( "red", diffSpeed < 0 );
	}

	[Events.Timer.OnStageStop]
	public void OnStopped( Strafe.Players.TimerEntity timer )
	{
		if ( timer.Stage != 0 ) return;

		TimeDiff.Text = "";
		SpeedDiff.Text = "";
	}

	[Events.Timer.OnStageStart]
	public void OnStageStart( Strafe.Players.TimerEntity timer )
	{
		if ( timer.Owner is not StrafePlayer pl ) return;
		if ( !pl.IsLocalPawn ) return;
		if ( timer.Stage != 0 ) return;

		if( CprEntity.TryGetDiff( 1, default, out var cprframe ) )
		{
			// TODO: Cpr[1].Velocity isn't our start speed, we actually don't even store
			// start speed anywhere yet
			var diff = pl.Velocity.Length - CprEntity.Current.Cpr[1].Velocity.Length;
			SpeedDiff.Text = $"{(int)diff} u/s";
			SpeedDiff.SetClass( "red", diff < 0 );
		}
	}

	[Events.Timer.OnStageComplete]
	public void OnStage( Strafe.Players.TimerEntity timer )
	{
		if ( timer.Owner is not StrafePlayer pl ) return;
		if ( !pl.IsLocalPawn ) return;

		BuildDiff( timer );
	}

}
