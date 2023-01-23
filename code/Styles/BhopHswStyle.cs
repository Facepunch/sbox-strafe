
class BhopHswStyle : BaseStyle
{

	// Theory about blocking non-HSW strafes while playing HSW:
	// Block S and W without A or D.
	// Block A and D without S or W.
	public override void Simulate( StrafeController ctrl )
	{
		base.Simulate( ctrl );

		if ( ctrl.Pawn is not StrafePlayer pl ) return;
		if( pl.TimerState == Strafe.Players.TimerEntity.States.Start ) return;

		if( pl.InputDirection.x == 0 )
		{
			pl.InputDirection = pl.InputDirection.WithY( 0 );
		}

		if ( pl.InputDirection.y == 0 )
		{
			pl.InputDirection = pl.InputDirection.WithX( 0 );
		}
	}

}
