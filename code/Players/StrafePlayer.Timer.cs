
using Sandbox;
using Sandbox.UI;
using Strafe.Map;

namespace Strafe.Players;

internal partial class StrafePlayer
{

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if( other is BaseZone zone )
		{
			Display( zone.GetType() + " has been entered" );
		}
	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );

		if ( other is BaseZone zone )
		{
			Display( zone.GetType() + " has been exited" );
		}
	}

	void Display( string message )
	{
		if ( IsServer ) return;

		ChatBox.AddInformation( message );
	}

}
