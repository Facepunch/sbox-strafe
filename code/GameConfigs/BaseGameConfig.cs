
namespace Strafe.GameConfigs;

internal class BaseGameConfig
{

	public virtual void OnPlayerSpawned( StrafePlayer pl )
	{
		Game.AssertServer();

	}

}
