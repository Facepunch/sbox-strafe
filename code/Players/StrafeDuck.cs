
using Sandbox;

namespace Strafe.Players;

internal class StrafeDuck : Duck
{

	public StrafeDuck( BasePlayerController controller ) : base( controller )
	{
	}

	public override void PreTick()
	{
		bool wants = Input.Down( InputButton.Duck );

		if ( wants != IsActive )
		{
			if ( wants ) TryDuck();
			else TryUnDuck();
		}

		if ( IsActive )
		{
			Controller.SetTag( "ducked" );
			Controller.EyeLocalPosition *= .4375f;
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

		if ( wasactive && !IsActive )
		{
		}
	}

}
