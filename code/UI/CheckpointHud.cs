
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
		if( TimeSinceShown > 8f )
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

		Add.Label( $"Time {timer.Timer.HumanReadable()}s", "row" );
		Add.Label( $"Jumps {timer.Jumps}", "row" );
		Add.Label( $"Strafes {timer.Strafes}", "row" );
	}

	[Events.Timer.OnStage]
	public void OnStage( TimerEntity timer )
	{
		if ( timer.Owner is not StrafePlayer pl ) return;
		if ( !pl.IsLocalPawn ) return;

		Rebuild( timer.Stage, pl.Stage( 0 ) );
		Style.Opacity = 1;
		TimeSinceShown = 0;
	}

}
