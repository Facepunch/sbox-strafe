
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

		var hash = HashCode.Combine( timer.Stage, timer.Timer );
		if ( hash == currenthash ) return;

		currenthash = hash;
		Rebuild( timer );
	}

	private void Rebuild( TimerEntity timer )
	{
		DeleteChildren( true );

		if( StrafeGame.Current.CourseType == CourseTypes.Linear )
		{
			Add.Label( $"CP #{timer.Stage}", "row" );
		}
		else
		{
			Add.Label( $"Stage #{timer.Stage}", "row" );
		}
		
		Add.Label( $"Time {timer.Timer.HumanReadable()}s", "row" );
		Add.Label( $"Jumps {timer.Jumps}", "row" );
		Add.Label( $"Strafes {timer.Strafes}", "row" );
	}

	private bool TryGetFrame( out TimerEntity timer )
	{
		timer = null;

		if ( Local.Pawn is not StrafePlayer pl ) return false;

		timer = pl.PreviousStage();

		return timer.IsValid();
	}

}
