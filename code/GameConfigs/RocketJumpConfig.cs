
namespace Strafe.GameConfigs;

internal class RocketJumpConfig : BaseGameConfig
{

	public override void OnPlayerSpawned( StrafePlayer pl )
	{
		base.OnPlayerSpawned( pl );

		Game.AssertServer();

		pl.Handheld = new RocketLauncher();
		pl.Handheld.Owner = pl;
		pl.Handheld.SetParent( pl );
		pl.Handheld.LocalPosition = 0;
		pl.Handheld.LocalRotation = Rotation.Identity;
	}

}
