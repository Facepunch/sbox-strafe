
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Strafe.Players;
using Strafe.Utility;
using System;

namespace Strafe.UI;

[Hud, UseTemplate]
internal class CheckpointHud : Panel
{

	private int currenthash = -69420;

	public override void Tick()
	{
		if ( !TryGetFrame( out var timer ) )
		{
			Style.Opacity = 0;
			return;
		}

		Style.Opacity = 1;

		var hash = HashCode.Combine( timer.Stage, timer.Timer, timer.Checkpoint );
		if ( hash == currenthash ) return;

		currenthash = hash;
		Rebuild( timer );
	}

	private void Rebuild( TimerEntity timer )
	{
		DeleteChildren( true );

		if( StrafeGame.Current.CourseType == CourseTypes.Linear )
		{
			Add.Label( $"CP #{timer.Checkpoint}", "row" );
		}
		else
		{
			Add.Label( $"Stage #{timer.Stage}", "row" );
		}
		
		Add.Label( $"Time {timer.Snapshot.Time.HumanReadable()}s", "row" );
		Add.Label( $"Jumps {timer.Snapshot.Jumps}", "row" );
		Add.Label( $"Strafes {timer.Snapshot.Strafes}", "row" );
	}

	private bool TryGetFrame( out TimerEntity timer )
	{
		timer = null;

		if ( Local.Pawn is not StrafePlayer pl ) return false;

		var stage = pl.CurrentStage();
		if ( !stage.IsValid() ) return false;

		switch ( StrafeGame.Current.CourseType )
		{
			case CourseTypes.Linear:
				if ( stage.Checkpoint == 0 ) 
					return false;
				break;
			case CourseTypes.Staged:
				if ( stage.State != TimerEntity.States.Complete )
					return false;
				break;
		}

		if ( stage.Stage == 0 && StrafeGame.Current.CourseType == CourseTypes.Staged ) return false;
		if ( stage.Checkpoint == 0 && StrafeGame.Current.CourseType == CourseTypes.Linear ) return false;

		timer = stage;

		return true;
	}

}
