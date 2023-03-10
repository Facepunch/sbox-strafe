
namespace Strafe.GameConfigs;

internal class Quake3Config : BaseGameConfig
{

	public override void OnPlayerSpawned( StrafePlayer pl )
	{
		base.OnPlayerSpawned( pl );

		pl.Controller = new QuakeController()
		{
			DefaultSpeed = 320,
			AutoJump = false,
			GroundFriction = 5
		};
	}

}
