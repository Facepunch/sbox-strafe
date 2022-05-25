
using Sandbox;

namespace Strafe.Players;

internal partial class StrafePlayer
{

	private Particles TrailParticle;

	public bool ToggleTrail()
	{
		if( TrailParticle == null )
		{
			TrailParticle = Particles.Create( "particles/gameplay/strafe_trail/strafe_trail.vpcf", this );
			return true;
		}
		else
		{
			TrailParticle.Destroy();
			TrailParticle = null;
			return false;
		}
	}

	[Event.Tick.Client]
	private void UpdateTrail()
	{
		if ( TrailParticle == null ) return;

		var spd = Velocity.Length.Remap( 0f, 1000, 0, 1 );
		TrailParticle.SetPosition( 10, new Vector3( spd, 0f, 0f ) );
	}

}
