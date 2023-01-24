
namespace Strafe;

class WOnlyStyle : BaseStyle
{

	public override void Simulate( StrafeController ctrl )
	{
		base.Simulate( ctrl );

		if ( ctrl.Pawn is not StrafePlayer pl ) return;
		if ( pl.TimerState == Strafe.Players.TimerEntity.States.Start ) return;

		pl.InputDirection = pl.InputDirection.WithY( 0 );

		if( pl.InputDirection.x != 1 )
		{
			pl.InputDirection = pl.InputDirection.WithX( 0 );
		}
	}

}
