
using Sandbox;
using System;

namespace Strafe.Players;

internal class StrafeDuck : Duck
{

	public StrafeDuck( BasePlayerController controller ) : base( controller )
	{
	}

	public override void PreTick()
	{
		bool wants = Input.Down( "Duck" );

		if ( wants != IsActive )
		{
			if ( wants ) TryDuck();
			else TryUnDuck();
		}

		if ( IsActive )
		{
			Controller.SetTag( "ducked" );
		}
	}

	protected override void TryDuck()
	{
		var wasactive = IsActive;
		base.TryDuck();

		if ( !wasactive && IsActive )
		{
			Controller.Position += Vector3.Up * 14;
		}
	}

	protected override void TryUnDuck()
	{
		var wasactive = IsActive;
		base.TryUnDuck();

		if ( wasactive && !IsActive && Controller.GroundEntity == null )
		{
			var distToGround = Controller.TraceBBox( Controller.Position, Controller.Position + Vector3.Down * 1000 ).Distance;
			var shift = MathF.Min( 14, distToGround );
			Controller.Position += Vector3.Down * shift;
		}
	}

	public override float GetWishSpeed()
	{
		if ( !IsActive ) return -1;
		return 88;
	}

}
