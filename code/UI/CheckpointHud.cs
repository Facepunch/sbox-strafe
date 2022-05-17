
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Strafe.Players;
using Strafe.Utility;

namespace Strafe.UI;

[Hud, UseTemplate]
internal class CheckpointHud : Panel
{

	private TimeSince TimeSinceShown;

	public override void Tick()
	{
		if( TimeSinceShown > 10f )
		{
			Style.Opacity = 0;
		}
	}

	private void Rebuild( int stage, TimerEntity timer )
	{
		DeleteChildren( true );

		if( stage == 0 )
		{
			Add.Label( "Map", "row" );
		}
		else
		{
			if ( StrafeGame.Current.CourseType == CourseTypes.Linear )
			{
				Add.Label( $"CP #{stage}", "row" );
			}
			else
			{
				Add.Label( $"Stage #{stage}", "row" );
			}
		}

		var snapshot = StrafeGame.Current.CourseType == CourseTypes.Staged
			? timer.CurrentFrame()
			: (Local.Pawn as StrafePlayer).Stage( 0 ).CurrentFrame();

		var diff = StrafeGame.Current.Diff( timer.Stage, snapshot );

		if ( diff != default )
		{
			AddClass( "cpr" );
			AddChild( new CprRow( $"Time {snapshot.Time.ToTime()}", diff.Time, CprRow.CprType.Time ) );
			AddChild( new CprRow( $"Jumps {snapshot.Jumps}", diff.Jumps, CprRow.CprType.Int ) );
			AddChild( new CprRow( $"Strafes {snapshot.Strafes}", diff.Strafes, CprRow.CprType.Int ) );
		}
		else
		{
			RemoveClass( "cpr" );
			Add.Label( $"Time {snapshot.Time.ToTime()}", "row" );
			Add.Label( $"Jumps {snapshot.Jumps}", "row" );
			Add.Label( $"Strafes {snapshot.Strafes}", "row" );
		}
	}

	[Events.Timer.OnStageComplete]
	public void OnStage( TimerEntity timer )
	{
		if ( timer.Owner is not StrafePlayer pl ) return;
		if ( !pl.IsLocalPawn ) return;

		Rebuild( timer.Stage, timer );
		Style.Opacity = 1;
		TimeSinceShown = 0;
	}

	private class CprRow : Panel
	{

		public CprRow( string title, float diff, CprType type )
		{
			Add.Label( title, "title" );

			if ( type == CprType.None ) return;

			var difftext = type switch
			{
				CprType.Time => diff.ToTime( true ),
				CprType.Int => (diff > 0 ? '+' : '-') + ((int)diff).ToString(),
				_ => string.Empty
			};

			var cprlbl = Add.Label( difftext, "diff" );

			if ( diff > 0 ) cprlbl.AddClass( "green" );
			else cprlbl.AddClass( "red" );
		}

		public enum CprType
		{
			None,
			Time,
			Int
		}

	}

}
